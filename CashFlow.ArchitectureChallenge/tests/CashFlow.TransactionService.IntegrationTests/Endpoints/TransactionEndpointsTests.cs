using CashFlow.ArchitectureChallenge.TestCommon.Fixtures;
using CashFlow.TransactionService.IntegrationTests.Web;
using FluentAssertions;
using System.Net;
using System.Net.Http.Json;

namespace CashFlow.TransactionService.IntegrationTests.Endpoints;

public sealed class TransactionEndpointsTests : IAsyncLifetime
{
    private readonly PostgreSqlContainerFixture _databaseFixture = new("cashflow_transaction_test");
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
    public async Task PostTransactions_ShouldReturn201_WhenRequestIsValid()
    {
        // Arrange
        var request = new
        {
            amount = 150m,
            type = 1,
            description = "Venda do dia"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturn400_WhenAmountIsInvalid()
    {
        // Arrange
        var request = new
        {
            amount = 0m,
            type = 1,
            description = "Venda inválida"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturn400_WhenTypeIsInvalid()
    {
        // Arrange
        var request = new
        {
            amount = 100m,
            type = 99,
            description = "Tipo inválido"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturn400_WhenDescriptionIsEmpty()
    {
        // Arrange
        var request = new
        {
            amount = 100m,
            type = 1,
            description = ""
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task PostTransactions_ShouldReturn201_AndReturnLocationHeader()
    {
        // Arrange
        var request = new
        {
            amount = 210m,
            type = 1,
            description = "Venda com location"
        };

        // Act
        var response = await _client.PostAsJsonAsync("/transactions", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        response.Headers.Location.Should().NotBeNull();
    }

    [Fact]
    public async Task PostTransactions_ShouldEchoCorrelationIdHeader()
    {
        // Arrange
        const string correlationId = "corr-int-001";

        var request = new
        {
            amount = 110m,
            type = 1,
            description = "Venda com correlation"
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
        response.Headers.TryGetValues("X-Correlation-Id", out var values).Should().BeTrue();
        values!.Single().Should().Be(correlationId);
    }
}