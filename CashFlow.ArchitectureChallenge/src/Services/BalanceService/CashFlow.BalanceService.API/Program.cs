using CashFlow.BalanceService.Application;
using CashFlow.BalanceService.Infrastructure;
using OpenTelemetry.Metrics;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
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

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

app.Run();
