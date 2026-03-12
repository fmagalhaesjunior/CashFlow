using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;

namespace CashFlow.ArchitectureChallenge.E2ETests.Fixtures;

public sealed class EndToEndEnvironmentFixture : IAsyncLifetime
{
    private readonly IContainer _postgresContainer;
    private readonly IContainer _rabbitMqContainer;

    public EndToEndEnvironmentFixture()
    {
        _postgresContainer = new ContainerBuilder("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "postgres")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(5432))
            .Build();

        _rabbitMqContainer = new ContainerBuilder("rabbitmq:3.13-management")
            .WithPortBinding(5672, true)
            .WithPortBinding(15672, true)
            .WithEnvironment("RABBITMQ_DEFAULT_USER", "guest")
            .WithEnvironment("RABBITMQ_DEFAULT_PASS", "guest")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(5672))
            .Build();
    }

    public string TransactionConnectionString { get; private set; } = string.Empty;
    public string BalanceConnectionString { get; private set; } = string.Empty;

    public string RabbitMqHost { get; private set; } = "localhost";
    public int RabbitMqPort { get; private set; }

    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();
        await _rabbitMqContainer.StartAsync();

        var postgresPort = _postgresContainer.GetMappedPublicPort(5432);
        RabbitMqPort = _rabbitMqContainer.GetMappedPublicPort(5672);

        var adminConnectionString =
            $"Host=localhost;Port={postgresPort};Database=postgres;Username=postgres;Password=postgres";

        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync();

            await using var createTransactionDb = connection.CreateCommand();
            createTransactionDb.CommandText = "CREATE DATABASE \"cashflow_transaction_e2e\";";
            await createTransactionDb.ExecuteNonQueryAsync();

            await using var createBalanceDb = connection.CreateCommand();
            createBalanceDb.CommandText = "CREATE DATABASE \"cashflow_balance_e2e\";";
            await createBalanceDb.ExecuteNonQueryAsync();
        }

        TransactionConnectionString =
            $"Host=localhost;Port={postgresPort};Database=cashflow_transaction_e2e;Username=postgres;Password=postgres";

        BalanceConnectionString =
            $"Host=localhost;Port={postgresPort};Database=cashflow_balance_e2e;Username=postgres;Password=postgres";
    }

    public async Task DisposeAsync()
    {
        await _rabbitMqContainer.DisposeAsync();
        await _postgresContainer.DisposeAsync();
    }
}