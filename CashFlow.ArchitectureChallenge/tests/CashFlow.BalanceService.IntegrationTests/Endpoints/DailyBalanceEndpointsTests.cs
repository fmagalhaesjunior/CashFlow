using CashFlow.ArchitectureChallenge.TestCommon.Fixtures;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BalanceService.Infrastructure.Persistence;
using CashFlow.BalanceService.IntegrationTests.Web;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;

namespace CashFlow.BalanceService.IntegrationTests.Endpoints;

public sealed class DailyBalanceEndpointsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _databaseFixture = new("cashflow_balance_test");
    private CustomBalanceWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        _factory = new CustomBalanceWebApplicationFactory(_databaseFixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReturn200_WhenBalanceExists()
    {
        // Arrange
        await SeedDailyBalanceAsync(new DateOnly(2026, 03, 11), 150m, 40m);

        // Act
        var response = await _client.GetAsync("/daily-balance/2026-03-11");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReturn404_WhenBalanceDoesNotExist()
    {
        // Act
        var response = await _client.GetAsync("/daily-balance/2026-03-12");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReturn400_WhenDateFormatIsInvalid()
    {
        // Act
        var response = await _client.GetAsync("/daily-balance/11-03-2026");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReturnCorrectPayload_WhenBalanceExists()
    {
        // Arrange
        await SeedDailyBalanceAsync(new DateOnly(2026, 03, 11), 200m, 50m);

        // Act
        var response = await _client.GetAsync("/daily-balance/2026-03-11");
        var payload = await response.Content.ReadAsStringAsync();

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        payload.Should().Contain("\"date\":\"2026-03-11\"");
        payload.Should().Contain("\"totalCredit\":200");
        payload.Should().Contain("\"totalDebit\":50");
        payload.Should().Contain("\"balance\":150");
    }

    private async Task SeedDailyBalanceAsync(DateOnly date, decimal credit, decimal debit)
    {
        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();

        var balance = DailyBalance.Create(date);
        if (credit > 0)
        {
            balance.ApplyCredit(credit);
        }

        if (debit > 0)
        {
            balance.ApplyDebit(debit);
        }

        dbContext.DailyBalances.Add(balance);
        await dbContext.SaveChangesAsync();
    }
}