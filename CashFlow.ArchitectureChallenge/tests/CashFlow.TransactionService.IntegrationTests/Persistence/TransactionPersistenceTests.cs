using CashFlow.ArchitectureChallenge.TestCommon.Fixtures;
using CashFlow.TransactionService.Infra.Persistence;
using CashFlow.TransactionService.IntegrationTests.Web;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using System.Net.Http.Json;

namespace CashFlow.TransactionService.IntegrationTests.Persistence;

public sealed class TransactionPersistenceTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _databaseFixture = new("cashflow_transaction_persistence_test");
    private CustomTransactionWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    public async Task InitializeAsync()
    {
        await _databaseFixture.InitializeAsync();
        _factory = new CustomTransactionWebApplicationFactory(_databaseFixture.ConnectionString);
        _client = _factory.CreateClient();
    }

    public async Task DisposeAsync()
    {
        _client.Dispose();
        await _factory.DisposeAsync();
        await _databaseFixture.DisposeAsync();
    }

    [Fact]
    public async Task PostTransactions_ShouldPersistTransactionInDatabase()
    {
        // Arrange
        var request = new
        {
            amount = 300m,
            type = 1,
            description = "Persist transaction"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        dbContext.Transactions.Should().HaveCount(1);
        dbContext.Transactions.Single().Amount.Should().Be(300m);
    }

    [Fact]
    public async Task PostTransactions_ShouldPersistOutboxMessageInDatabase()
    {
        // Arrange
        var request = new
        {
            amount = 180m,
            type = 1,
            description = "Persist outbox"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        dbContext.OutboxEvents.Should().HaveCount(1);
    }

    [Fact]
    public async Task PostTransactions_ShouldPersistOutboxWithPendingStatus()
    {
        // Arrange
        var request = new
        {
            amount = 90m,
            type = 1,
            description = "Pending outbox"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var outbox = dbContext.OutboxEvents.Single();
        outbox.Status.ToString().Should().Be("Pending");
    }

    [Fact]
    public async Task PostTransactions_ShouldPersistOutboxWithCorrelationId()
    {
        // Arrange
        const string correlationId = "corr-persist-001";

        var request = new
        {
            amount = 99m,
            type = 1,
            description = "Correlation persist"
        };

        var httpRequest = new HttpRequestMessage(HttpMethod.Post, "/transactions")
        {
            Content = JsonContent.Create(request)
        };

        httpRequest.Headers.Add("X-Correlation-Id", correlationId);

        // Act
        var response = await _client.SendAsync(httpRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var outbox = dbContext.OutboxEvents.Single();
        outbox.CorrelationId.Should().Be(correlationId);
    }

    [Fact]
    public async Task PostTransactions_ShouldPersistOutboxWithEventTypeTransactionCreatedIntegrationEvent()
    {
        // Arrange
        var request = new
        {
            amount = 70m,
            type = 1,
            description = "Event type persist"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        using var scope = _factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

        var outbox = dbContext.OutboxEvents.Single();
        outbox.EventType.Should().Be("TransactionCreatedIntegrationEvent");
    }
}