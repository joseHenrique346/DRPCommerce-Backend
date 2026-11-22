using SharedCommerce.Domain.Exceptions;

namespace StoreCommerce.Application.Identity;

public static class AccountPasswordPolicy
{
    public static bool IsValid(string? password)
    {
        return !string.IsNullOrEmpty(password) && password.Length is >= 12 and <= 128;
    }

    public static void Validate(string password)
    {
        if (!IsValid(password))
            throw new SecurityFailure(400, "Identity.PasswordPolicy", "A senha deve ter entre 12 e 128 caracteres.");
    }
}
