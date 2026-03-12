using CashFlow.BalanceService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace CashFlow.BalanceService.IntegrationTests.Web;

public sealed class CustomBalanceWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly string _connectionString;

    public CustomBalanceWebApplicationFactory(string connectionString)
    {
        _connectionString = connectionString;
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var config = new Dictionary<string, string?>
            {
                ["ConnectionStrings:BalanceDb"] = _connectionString,
                ["RabbitMq:HostName"] = "localhost",
                ["RabbitMq:Port"] = "5672",
                ["RabbitMq:UserName"] = "guest",
                ["RabbitMq:Password"] = "guest",
                ["RabbitMq:VirtualHost"] = "/",
                ["RabbitMq:ExchangeName"] = "cashflow.events",
                ["RabbitMq:QueueName"] = "balance.transaction.created",
                ["RabbitMq:RoutingKey"] = "transaction.created"
            };

            configBuilder.AddInMemoryCollection(config);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IHostedService>();

            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<BalanceDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            services.AddDbContext<BalanceDbContext>(options =>
            {
                options.UseNpgsql(_connectionString);
            });

            using var scope = services.BuildServiceProvider().CreateScope();
            var dbContext = scope.ServiceProvider.GetRequiredService<BalanceDbContext>();
            dbContext.Database.EnsureCreated();
        });
    }
}