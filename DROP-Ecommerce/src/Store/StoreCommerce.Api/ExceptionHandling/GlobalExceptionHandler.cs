using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.ExceptionHandling;

public sealed class GlobalExceptionHandler(
    IProblemDetailsService problemDetailsService,
    ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext httpContext,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (Commerce.Security.SecurityErrors.Classify(exception) is { } expected)
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
