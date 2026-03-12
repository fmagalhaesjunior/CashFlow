using CashFlow.TransactionService.API.Endpoints;
using CashFlow.TransactionService.API.Middlewares;
using CashFlow.TransactionService.Application;
using CashFlow.TransactionService.Infra;
using Scalar.AspNetCore;

namespace CashFlow.TransactionService.API;

public partial class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddOpenApi();

        builder.Services.AddTransactionApplication();
        builder.Services.AddTransactionInfrastructure(builder.Configuration);

        var app = builder.Build();

        app.UseMiddleware<ExceptionHandlingMiddleware>();

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
        }

        app.MapTransactionEndpoints();

        app.MapGet("/health", () => Results.Ok(new { status = "healthy" }));

        app.Run();
    }
}