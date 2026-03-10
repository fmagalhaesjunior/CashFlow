using CashFlow.BalanceService.Application.UseCases.ProcessTransactionCreated;
using Microsoft.Extensions.DependencyInjection;

namespace CashFlow.BalanceService.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddBalanceApplication(this IServiceCollection services)
    {
        services.AddScoped<ProcessTransactionCreatedService>();
        return services;
    }
}