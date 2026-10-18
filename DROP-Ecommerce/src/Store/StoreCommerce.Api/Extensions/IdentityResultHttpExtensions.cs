using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Api.Extensions;

public static class IdentityResultHttpExtensions
{
    public static IActionResult ToIdentityActionResult<T>(this ControllerBase controller, Result<T> result)
    {
        if (result.IsSuccess)
            return controller.Ok(result.Content);

        var error = result.Errors[0];
        var status = error.Type switch
        {
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.Forbidden => StatusCodes.Status403Forbidden,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Unavailable => StatusCodes.Status503ServiceUnavailable,
            _ => StatusCodes.Status400BadRequest
        };

        var problemDetails = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: status,
            title: error.Message);

        problemDetails.Extensions["code"] = error.Code;
        problemDetails.Extensions["traceId"] = controller.HttpContext.TraceIdentifier;

        return new ObjectResult(problemDetails)
        {
            StatusCode = status,
            ContentTypes =
            {
                "application/problem+json"
            }
        };
    }
}
