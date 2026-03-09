using CashFlow.BuildingBlocks.Domain.Abstractions;
using FluentValidation;
using System.Net;
using System.Text.Json;

namespace CashFlow.TransactionService.API.Middlewares;

public sealed class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(
        RequestDelegate next,
        ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (ValidationException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
            context.Response.ContentType = "application/json";

            var errors = ex.Errors
                .Select(x => new
                {
                    x.PropertyName,
                    x.ErrorMessage
                });

            var response = new
            {
                message = "Validation failed.",
                errors
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (DomainException ex)
        {
            context.Response.StatusCode = (int)HttpStatusCode.UnprocessableEntity;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = ex.Message
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error while processing request.");

            context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
            context.Response.ContentType = "application/json";

            var response = new
            {
                message = "An unexpected error occurred."
            };

            await context.Response.WriteAsync(JsonSerializer.Serialize(response));
        }
    }
}