using CashFlow.ArchitectureChallenge.TestCommon.Fixtures;
using CashFlow.BalanceService.Application.Abstractions.Queries;
using CashFlow.BalanceService.Domain.Entities;
using CashFlow.BalanceService.Infrastructure.Persistence;
using CashFlow.BalanceService.IntegrationTests.Web;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.BalanceService.IntegrationTests.Persistence;

public sealed class DailyBalanceReadTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _databaseFixture = new("cashflow_balance_read_test");
    private CustomBalanceWebApplicationFactory _factory = null!;

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        _factory = new CustomBalanceWebApplicationFactory(_databaseFixture.ConnectionString);
    }

    public async Task DisposeAsync()
    {
        await _factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReadPersistedBalanceFromDatabase()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();

            var balance = DailyBalance.Create(new DateOnly(2026, 03, 11));
            balance.ApplyCredit(100m);
            balance.ApplyDebit(30m);

            dbContext.DailyBalances.Add(balance);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using var readScope = _factory.Services.CreateScope();
        var repository = readScope.ServiceProvider.GetRequiredService<IDailyBalanceReadRepository>();

        var result = await repository.GetByDateAsync(new DateOnly(2026, 03, 11), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
    }

    [Fact]
    public async Task GetDailyBalance_ShouldReturnExpectedCreditDebitAndBalance()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();

            var balance = DailyBalance.Create(new DateOnly(2026, 03, 11));
            balance.ApplyCredit(300m);
            balance.ApplyDebit(120m);

            dbContext.DailyBalances.Add(balance);
            await dbContext.SaveChangesAsync();
        }

        // Act
        using var readScope = _factory.Services.CreateScope();
        var repository = readScope.ServiceProvider.GetRequiredService<IDailyBalanceReadRepository>();

        var result = await repository.GetByDateAsync(new DateOnly(2026, 03, 11), CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result!.Date.Should().Be(new DateOnly(2026, 03, 11));
        result.TotalCredit.Should().Be(300m);
        result.TotalDebit.Should().Be(120m);
        result.Balance.Should().Be(180m);
    }
}