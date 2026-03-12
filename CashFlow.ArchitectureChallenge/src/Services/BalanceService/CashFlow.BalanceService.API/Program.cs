using CashFlow.BalanceService.API.Endpoints;
using CashFlow.BalanceService.Application;
using CashFlow.BalanceService.Infrastructure;
using OpenTelemetry.Metrics;
using Scalar.AspNetCore;

namespace CashFlow.BalanceService.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddBalanceApplication();
        builder.Services.AddBalanceInfrastructure(builder.Configuration);

        builder.Services.AddOpenTelemetry()
            .WithMetrics(metrics =>
            {
                metrics.AddMeter("CashFlow.BalanceService.Consumer");
                metrics.AddAspNetCoreInstrumentation();
                metrics.AddRuntimeInstrumentation();
            });

        var app = builder.Build();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));
        app.MapDailyBalanceEndpoints();

        app.Run();
    }
}