using MediatR;
using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Result;

public static class MediatorResultExtensions
{
    public static async Task<T> SendRequiredAsync<T>(this ISender sender, IRequest<Result<T>> request, CancellationToken ct = default)
    {
        var result = await sender.Send(request, ct);

        if (result.IsSuccess)
            return result.Content!;

        var error = result.Errors[0];
        var status = error.Type switch
        {
            ErrorType.Unauthorized => 401,
            ErrorType.Forbidden => 403,
            ErrorType.NotFound => 404,
            ErrorType.Conflict => 409,
            ErrorType.Unavailable => 503,
            _ => 400
        };

        throw new SecurityFailure(status, error.Code, error.Message);
    }
}
