using CashFlow.TransactionService.API.Contracts;
using CashFlow.TransactionService.Application.UseCases.CreateTransaction;

namespace CashFlow.TransactionService.API.Mappers;

public static class TransactionRequestMapper
{
    public static CreateTransactionCommand ToCommand(CreateTransactionRequest request)
    {
        return new CreateTransactionCommand
        {
            Amount = request.Amount,
            Type = request.Type,
            Description = request.Description
        };
    }
}