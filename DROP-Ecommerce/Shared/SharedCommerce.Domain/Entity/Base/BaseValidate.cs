using SharedCommerce.Domain.Exceptions;
namespace SharedCommerce.Domain.Entity.Base;

public static class BaseValidate
{
    public static void ValidatePositive(long value, string propertyName,
        string code = "Appearance.InvalidProperty", string? message = null)
    {
        if (value <= 0)
            throw new AppearanceFailure(400, code, message ?? $"{propertyName} deve ser maior que zero.");
    }

    public static void ValidatePositiveOrZero(long value, string propertyName)
    {
        if (value < 0)
            throw new AppearanceFailure(400, "Appearance.InvalidProperty", $"{propertyName} não pode ser negativo.");
    }

    public static void ValidateNullablePositive(long? value, string propertyName)
    {
        if (value.HasValue)
            ValidatePositive(value.Value, propertyName);
    }

    public static void ValidateNotNullOrEmpty(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new AppearanceFailure(400, "Appearance.InvalidProperty", $"{propertyName} não pode ser vazio.");
    }

    public static void ValidateMaxLength(string? value, int maxLength, string propertyName)
    {
        if (value is not null && value.Length > maxLength)
            throw new AppearanceFailure(400, "Appearance.InvalidProperty", $"{propertyName} deve ter no máximo {maxLength} caracteres.");
    }

    public static void ValidateDate(DateTime value, string propertyName)
    {
        if (value == default)
            throw new AppearanceFailure(400, "Appearance.InvalidProperty", $"{propertyName} deve ser uma data válida.");
    }
}
