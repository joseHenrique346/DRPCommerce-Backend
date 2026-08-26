using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Api.Extensions;

public static class ResultHttpExtensions
{
    private const string ProblemContentType = "application/problem+json";

    public static IActionResult ToActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(result);

        if (result.Errors.Count == 0)
        {
            return CreateProblemResult(
                controller,
                ErrorType.Failure,
                "Request.Failure",
                "A requisição não pôde ser processada.");
        }

        var primaryError = result.Errors[0];

        if (primaryError.Type == ErrorType.Validation)
            return CreateValidationProblemResult(controller, result.Errors);

        return CreateProblemResult(
            controller,
            primaryError.Type,
            primaryError.Code,
            primaryError.Message);
    }

    private static IActionResult CreateValidationProblemResult(
        ControllerBase controller,
        IReadOnlyList<Error> errors)
    {
        var validationErrors = errors
            .GroupBy(error => string.IsNullOrWhiteSpace(error.Code) ? "Validation" : error.Code)
            .ToDictionary(
                group => group.Key,
                group => group.Select(error => error.Message).ToArray());

        var primaryError = errors[0];
        var problemDetails = new ValidationProblemDetails(validationErrors)
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Erro de validação",
            Detail = "Um ou mais erros de validação ocorreram.",
            Type = GetProblemType(StatusCodes.Status400BadRequest)
        };

        problemDetails.Extensions["code"] = primaryError.Code;
        problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

        return CreateObjectResult(problemDetails, StatusCodes.Status400BadRequest);
    }

    private static IActionResult CreateProblemResult(
        ControllerBase controller,
        ErrorType errorType,
        string code,
        string message)
    {
        var statusCode = GetStatusCode(errorType);
        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = GetTitle(errorType),
            Detail = message,
            Type = GetProblemType(statusCode)
        };

        problemDetails.Extensions["code"] = code;
        problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

        return CreateObjectResult(problemDetails, statusCode);
    }

    private static ObjectResult CreateObjectResult(ProblemDetails problemDetails, int statusCode)
    {
        var result = new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };

        result.ContentTypes.Add(ProblemContentType);
        return result;
    }

    private static int GetStatusCode(ErrorType errorType) => errorType switch
    {
        ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
        ErrorType.Forbidden => StatusCodes.Status403Forbidden,
        ErrorType.NotFound => StatusCodes.Status404NotFound,
        ErrorType.Conflict => StatusCodes.Status409Conflict,
        ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
        ErrorType.Validation or ErrorType.Failure => StatusCodes.Status400BadRequest,
        _ => StatusCodes.Status400BadRequest
    };

    private static string GetTitle(ErrorType errorType) => errorType switch
    {
        ErrorType.Validation => "Erro de validação",
        ErrorType.Unauthorized => "Não autorizado",
        ErrorType.Forbidden => "Acesso negado",
        ErrorType.NotFound => "Recurso não encontrado",
        ErrorType.Conflict => "Conflito",
        ErrorType.Unavailable => "Serviço indisponível",
        _ => "Requisição inválida"
    };

    private static string GetProblemType(int statusCode) => $"https://httpstatuses.com/{statusCode}";
}
