using CashFlow.TransactionService.API.Endpoints;
using CashFlow.TransactionService.API.Middlewares;
using CashFlow.TransactionService.Application;
using CashFlow.TransactionService.Infra;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddTransactionApplication();
builder.Services.AddTransactionInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseAuthorization();

app.MapControllers();

app.MapTransactionEndpoints();

app.Run();
