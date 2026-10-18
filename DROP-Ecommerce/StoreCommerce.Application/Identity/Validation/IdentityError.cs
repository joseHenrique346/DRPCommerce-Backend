using Commerce.Security;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Identity;

public static class IdentityError
{
    public static Error FromFailure(SecurityFailure failure)
    {
        return failure.Status switch
        {
            400 => Error.Validation(failure.Code, failure.Message),
            401 => Error.Unauthorized(failure.Code, failure.Message),
            403 => Error.Forbidden(failure.Code, failure.Message),
            404 => Error.NotFound(failure.Code, failure.Message),
            409 => Error.Conflict(failure.Code, failure.Message),
            503 => Error.Unavailable(failure.Code, failure.Message),
            _ => throw new ArgumentOutOfRangeException(nameof(failure), failure.Status, "Status de identidade não mapeado.")
        };
    }
}
