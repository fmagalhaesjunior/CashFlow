using CashFlow.BuildingBlocks.Application.Messaging;
using CashFlow.TransactionService.Application.UseCases.CreateTransaction;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.TransactionService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddTransactionApplication(this IServiceCollection services)
    {
        services.AddScoped<
            ICommandHandler<CreateTransactionCommand, CreateTransactionResponse>,
            CreateTransactionCommandHandler>();

        services.AddScoped<IValidator<CreateTransactionCommand>, CreateTransactionCommandValidator>();

        return services;
    }
}