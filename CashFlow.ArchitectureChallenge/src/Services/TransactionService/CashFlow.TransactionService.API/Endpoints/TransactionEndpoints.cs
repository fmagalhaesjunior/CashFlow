using CashFlow.BuildingBlocks.Application.Messaging;
using CashFlow.TransactionService.API.Contracts;
using CashFlow.TransactionService.API.Mappers;
using CashFlow.TransactionService.Application.UseCases.CreateTransaction;
using FluentValidation;

namespace CashFlow.TransactionService.API.Endpoints;

public static class TransactionEndpoints
{
    public static IEndpointRouteBuilder MapTransactionEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/transactions", async (
            CreateTransactionRequest request,
            IValidator<CreateTransactionCommand> validator,
            ICommandHandler<CreateTransactionCommand, CreateTransactionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var command = TransactionRequestMapper.ToCommand(request);

            await validator.ValidateAndThrowAsync(command, cancellationToken);

            var response = await handler.Handle(command, cancellationToken);

            return Results.Created($"/transactions/{response.TransactionId}", response);
        })
        .WithName("CreateTransaction")
        .WithTags("Transactions")
        .Produces<CreateTransactionResponse>(StatusCodes.Status201Created)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status422UnprocessableEntity)
        .Produces(StatusCodes.Status500InternalServerError);

        return endpoints;
    }
}