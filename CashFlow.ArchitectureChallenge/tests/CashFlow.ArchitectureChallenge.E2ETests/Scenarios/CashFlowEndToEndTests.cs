using CashFlow.ArchitectureChallenge.E2ETests.Fixtures;
using CashFlow.ArchitectureChallenge.E2ETests.Web;
using CashFlow.BalanceService.Infrastructure.Persistence;
using CashFlow.TransactionService.Infra.Persistence;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using RabbitMQ.Client;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace CashFlow.ArchitectureChallenge.E2ETests.Scenarios;

public sealed class CashFlowEndToEndTests : IAsyncLifetime
{
    private static readonly DateTime FixedUtcNow =
        new(2026, 03, 11, 10, 00, 00, DateTimeKind.Utc);

    private readonly EndToEndEnvironmentFixture _environment = new();

    private CustomTransactionE2EWebApplicationFactory _transactionFactory = null!;
    private CustomBalanceE2EWebApplicationFactory _balanceFactory = null!;

    private HttpClient _transactionClient = null!;
    private HttpClient _balanceClient = null!;

    public async Task InitializeAsync()
    {
        await _environment.InitializeAsync();

        _balanceFactory = new CustomBalanceE2EWebApplicationFactory(
            _environment.BalanceConnectionString,
            _environment.RabbitMqHost,
            _environment.RabbitMqPort);

        _transactionFactory = new CustomTransactionE2EWebApplicationFactory(
            _environment.TransactionConnectionString,
            _environment.RabbitMqHost,
            _environment.RabbitMqPort,
            FixedUtcNow);

        _balanceClient = _balanceFactory.CreateClient();
        _transactionClient = _transactionFactory.CreateClient();

        await WarmUpApisAsync();
        await WaitUntilBalanceConsumerQueueIsReadyAsync(
            _environment.RabbitMqHost,
            _environment.RabbitMqPort,
            TimeSpan.FromSeconds(30));
    }

    public async Task DisposeAsync()
    {
        _transactionClient.Dispose();
        _balanceClient.Dispose();

        await _transactionFactory.DisposeAsync();
        await _balanceFactory.DisposeAsync();
        await _environment.DisposeAsync();
    }

    [Fact]
    public async Task EndToEnd_ShouldProcessTransactions_AndReturnConsolidatedDailyBalance()
    {
        const string date = "2026-03-11";

        var creditRequest = new
        {
            amount = 150m,
            type = 1,
            description = "Venda do dia"
        };

        var debitRequest = new
        {
            amount = 40m,
            type = 2,
            description = "Pagamento fornecedor"
        };

        var creditResponse = await _transactionClient.PostAsJsonAsync("/transactions", creditRequest);
        var debitResponse = await _transactionClient.PostAsJsonAsync("/transactions", debitRequest);

        creditResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        debitResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        await WaitUntilOutboxIsProcessedAsync(expectedProcessedCount: 2, timeout: TimeSpan.FromSeconds(30));
        await WaitUntilProcessedEventsExistAsync(expectedCount: 2, timeout: TimeSpan.FromSeconds(30));

        var result = await WaitUntilAsync(async () =>
        {
            var response = await _balanceClient.GetAsync($"/daily-balance/{date}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(content);
        },
        timeout: TimeSpan.FromSeconds(40),
        pollInterval: TimeSpan.FromSeconds(1));

        result.Should().NotBeNull();

        var root = result!.RootElement;

        root.GetProperty("date").GetString().Should().Be(date);
        root.GetProperty("totalCredit").GetDecimal().Should().Be(150m);
        root.GetProperty("totalDebit").GetDecimal().Should().Be(40m);
        root.GetProperty("balance").GetDecimal().Should().Be(110m);
    }

    [Fact]
    public async Task EndToEnd_ShouldAccumulateMultipleTransactionsCorrectly()
    {
        const string date = "2026-03-11";

        var requests = new[]
        {
            new { amount = 100m, type = 1, description = "Credito 1" },
            new { amount = 50m, type = 1, description = "Credito 2" },
            new { amount = 30m, type = 2, description = "Debito 1" },
            new { amount = 20m, type = 2, description = "Debito 2" }
        };

        foreach (var request in requests)
        {
            var response = await _transactionClient.PostAsJsonAsync("/transactions", request);
            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }

        await WaitUntilOutboxIsProcessedAsync(expectedProcessedCount: 4, timeout: TimeSpan.FromSeconds(30));
        await WaitUntilProcessedEventsExistAsync(expectedCount: 4, timeout: TimeSpan.FromSeconds(30));

        var result = await WaitUntilAsync(async () =>
        {
            var response = await _balanceClient.GetAsync($"/daily-balance/{date}");

            if (response.StatusCode != HttpStatusCode.OK)
            {
                return null;
            }

            var content = await response.Content.ReadAsStringAsync();
            return JsonDocument.Parse(content);
        },
        timeout: TimeSpan.FromSeconds(40),
        pollInterval: TimeSpan.FromSeconds(1));

        result.Should().NotBeNull();

        var root = result!.RootElement;
        root.GetProperty("date").GetString().Should().Be(date);
        root.GetProperty("totalCredit").GetDecimal().Should().Be(150m);
        root.GetProperty("totalDebit").GetDecimal().Should().Be(50m);
        root.GetProperty("balance").GetDecimal().Should().Be(100m);
    }

    private async Task WarmUpApisAsync()
    {
        var balanceHealth = await _balanceClient.GetAsync("/health");
        var transactionHealth = await _transactionClient.GetAsync("/health");

        balanceHealth.EnsureSuccessStatusCode();
        transactionHealth.EnsureSuccessStatusCode();
    }

    private static async Task WaitUntilBalanceConsumerQueueIsReadyAsync(
        string host,
        int port,
        TimeSpan timeout)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            try
            {
                var factory = new ConnectionFactory
                {
                    HostName = host,
                    Port = port,
                    UserName = "guest",
                    Password = "guest",
                    VirtualHost = "/"
                };

                await using var connection = await factory.CreateConnectionAsync();
                await using var channel = await connection.CreateChannelAsync();

                await channel.QueueDeclarePassiveAsync("balance.transaction.created");
                return;
            }
            catch
            {
                await Task.Delay(500);
            }
        }

        throw new TimeoutException(
            "BalanceService consumer queue was not declared within the expected time.");
    }

    private async Task WaitUntilOutboxIsProcessedAsync(
        int expectedProcessedCount,
        TimeSpan timeout)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            using var scope = _transactionFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<TransactionDbContext>();

            var processedCount = dbContext.OutboxEvents.Count(x => x.Status.ToString() == "Processed");

            if (processedCount >= expectedProcessedCount)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException(
            $"Outbox events were not processed within the expected time. Expected at least {expectedProcessedCount} processed events.");
    }

    private async Task WaitUntilProcessedEventsExistAsync(
        int expectedCount,
        TimeSpan timeout)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            using var scope = _balanceFactory.Services.CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();

            var count = dbContext.ProcessedEvents.Count();

            if (count >= expectedCount)
            {
                return;
            }

            await Task.Delay(500);
        }

        throw new TimeoutException(
            $"Processed events were not created within the expected time. Expected at least {expectedCount}.");
    }

    private static async Task<JsonDocument?> WaitUntilAsync(
        Func<Task<JsonDocument?>> action,
        TimeSpan timeout,
        TimeSpan pollInterval)
    {
        var startedAt = DateTime.UtcNow;

        while (DateTime.UtcNow - startedAt < timeout)
        {
            var result = await action();

            if (result is not null)
            {
                return result;
            }

            await Task.Delay(pollInterval);
        }

        return null;
    }
}