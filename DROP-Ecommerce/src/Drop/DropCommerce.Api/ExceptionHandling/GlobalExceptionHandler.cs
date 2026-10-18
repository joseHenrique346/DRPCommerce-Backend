using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        var classified = exception is DropCommerce.Infrastructure.Data.Integrations.Store.Exceptions.StoreIntegrationException
            ? new Commerce.Security.SecurityFailure(503, "Store.Unavailable", "Integração com a loja indisponível.")
            : Commerce.Security.SecurityErrors.Classify(exception);
        if (classified is { } expected)
        {
            httpContext.Response.StatusCode = expected.Status;
            return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
            {
                HttpContext = httpContext,
                ProblemDetails = new ProblemDetails { Status = expected.Status, Title = expected.Message,
                    Extensions = { ["code"] = expected.Code, ["traceId"] = httpContext.TraceIdentifier } }
            });
        }
        logger.LogError("Erro inesperado do tipo {ExceptionType}. Trace {TraceId}.", exception.GetType().Name, httpContext.TraceIdentifier);

        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        var problemDetails = new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Erro interno do servidor",
            Detail = "Ocorreu um erro inesperado ao processar a solicitação.",
            Type = "https://httpstatuses.com/500"
        };

        problemDetails.Extensions["code"] = "Server.Unexpected";
        problemDetails.Extensions["traceId"] = httpContext.TraceIdentifier;

        return await problemDetailsService.TryWriteAsync(new ProblemDetailsContext
        {
            HttpContext = httpContext,
            ProblemDetails = problemDetails,
            Exception = exception
        });
    }
}