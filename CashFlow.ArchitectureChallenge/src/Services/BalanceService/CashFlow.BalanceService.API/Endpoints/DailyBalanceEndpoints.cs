using CashFlow.BalanceService.API.Mappers;
using CashFlow.BalanceService.Application.Queries.GetDailyBalance;

namespace CashFlow.BalanceService.API.Endpoints;

public static class DailyBalanceEndpoints
{
    public static IEndpointRouteBuilder MapDailyBalanceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapGet("/daily-balance/{date}", async (
            string date,
            GetDailyBalanceQueryService queryService,
            CancellationToken cancellationToken) =>
        {
            if (!DateOnly.TryParseExact(
                    date,
                    "yyyy-MM-dd",
                    out var parsedDate))
            {
                return Results.BadRequest(new
                {
                    message = "Invalid date format. Use yyyy-MM-dd."
                });
            }

            var result = await queryService.ExecuteAsync(parsedDate, cancellationToken);

            return result is null
                ? Results.NotFound(new
                {
                    message = "Daily balance not found for the specified date."
                })
                : Results.Ok(DailyBalanceResponseMapper.ToHttpResponse(result));
        })
        .WithName("GetDailyBalance")
        .WithTags("Daily Balance")
        .Produces(StatusCodes.Status200OK)
        .Produces(StatusCodes.Status400BadRequest)
        .Produces(StatusCodes.Status404NotFound);

        return endpoints;
    }
}