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
            HttpContext httpContext,
            CreateTransactionRequest request,
            IValidator<CreateTransactionCommand> validator,
            ICommandHandler<CreateTransactionCommand, CreateTransactionResponse> handler,
            CancellationToken cancellationToken) =>
        {
            var correlationId = httpContext.Request.Headers["X-Correlation-Id"].FirstOrDefault()
                ?? Guid.NewGuid().ToString("N");

            var command = TransactionRequestMapper.ToCommand(request, correlationId);

            await validator.ValidateAndThrowAsync(command, cancellationToken);

            var response = await handler.Handle(command, cancellationToken);

            httpContext.Response.Headers["X-Correlation-Id"] = correlationId;

            return Results.Created($"/transactions/{response.TransactionId}", response);
        })
        .WithName("CreateTransaction")
        .WithTags("Transactions");

        return endpoints;
    }
}