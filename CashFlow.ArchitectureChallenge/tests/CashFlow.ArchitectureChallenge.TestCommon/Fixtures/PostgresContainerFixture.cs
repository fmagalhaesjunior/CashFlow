using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Containers;
using Npgsql;

namespace CashFlow.ArchitectureChallenge.TestCommon.Fixtures;

public sealed class PostgreSqlContainerFixture : IAsyncLifetime
{
    private readonly string _databaseName;
    private readonly IContainer _container;

    public PostgreSqlContainerFixture(string databaseName)
    {
        _databaseName = databaseName;

        _container = new ContainerBuilder("postgres:16-alpine")
            .WithPortBinding(5432, true)
            .WithEnvironment("POSTGRES_USER", "postgres")
            .WithEnvironment("POSTGRES_PASSWORD", "postgres")
            .WithEnvironment("POSTGRES_DB", "postgres")
            .WithWaitStrategy(
                Wait.ForUnixContainer()
                    .UntilInternalTcpPortIsAvailable(5432))
            .Build();
    }

    public string ConnectionString { get; private set; } = string.Empty;

    public async Task InitializeAsync()
    {
        await _container.StartAsync();

        var mappedPort = _container.GetMappedPublicPort(5432);

        var adminConnectionString =
            $"Host=localhost;Port={mappedPort};Database=postgres;Username=postgres;Password=postgres";

        await using (var connection = new NpgsqlConnection(adminConnectionString))
        {
            await connection.OpenAsync();

            await using var command = connection.CreateCommand();
            command.CommandText = $"CREATE DATABASE \"{_databaseName}\";";
            await command.ExecuteNonQueryAsync();
        }

        ConnectionString =
            $"Host=localhost;Port={mappedPort};Database={_databaseName};Username=postgres;Password=postgres";
    }

    public async Task DisposeAsync()
    {
        await _container.DisposeAsync();
    }
}