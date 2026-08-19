using System.Text.RegularExpressions;

namespace StoreCommerce.Domain.Entity.Base;

public static class BaseValidate
{
    #region String Validation
    public static void ValidateNotNull(string? value, string propertyName)
    {
        if (value is null)
            throw new ArgumentException($"{propertyName} cannot be null.");
    }

    public static void ValidateNotEmpty(string? value, string propertyName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{propertyName} cannot be empty or whitespace.");
    }

    public static void ValidateNotNullOrEmpty(string? value, string propertyName)
    {
        ValidateNotNull(value, propertyName);
        ValidateNotEmpty(value, propertyName);
    }

    public static void ValidateMinLength(string? value, int minLength, string propertyName)
    {
        if (value is not null && value.Length < minLength)
            throw new ArgumentException($"{propertyName} must have at least {minLength} characters.");
    }

    public static void ValidateMaxLength(string? value, int maxLength, string propertyName)
    {
        if (value is not null && value.Length > maxLength)
            throw new ArgumentException($"{propertyName} must have at most {maxLength} characters.");
    }

    public static void ValidateLength(string? value, int minLength, int maxLength, string propertyName)
    {
        ValidateMinLength(value, minLength, propertyName);
        ValidateMaxLength(value, maxLength, propertyName);
    }

    public static void ValidateEmailFormat(string? value, string propertyName)
    {
        if (value is null) return;
        if (!Regex.IsMatch(value, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException($"{propertyName} has an invalid email format.");
    }

    public static void ValidateUrlFormat(string? value, string propertyName)
    {
        if (value is null) return;
        if (!Uri.TryCreate(value, UriKind.Absolute, out _))
            throw new ArgumentException($"{propertyName} has an invalid URL format.");
    }

    public static void ValidateZipCodeFormat(string? value, string propertyName)
    {
        if (value is null) return;
        if (!Regex.IsMatch(value, @"^\d{5}(-\d{4})?$|^\d{8}$"))
            throw new ArgumentException($"{propertyName} has an invalid zip code format.");
    }

    public static void ValidatePhoneFormat(string? value, string propertyName)
    {
        if (value is null) return;
        if (!Regex.IsMatch(value, @"^\+?[\d\s\-()]{7,20}$"))
            throw new ArgumentException($"{propertyName} has an invalid phone format.");
    }
    #endregion

    #region Long Validation
    public static void ValidatePositive(long value, string propertyName)
    {
        if (value <= 0)
            throw new ArgumentException($"{propertyName} must be a positive number.");
    }

    public static void ValidatePositiveOrZero(long value, string propertyName)
    {
        if (value < 0)
            throw new ArgumentException($"{propertyName} must be a positive number or zero.");
    }

    public static void ValidateGreaterThan(long value, long minValue, string propertyName)
    {
        if (value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }

    public static void ValidateLessThan(long value, long maxValue, string propertyName)
    {
        if (value >= maxValue)
            throw new ArgumentException($"{propertyName} must be less than {maxValue}.");
    }

    public static void ValidateRange(long value, long minValue, long maxValue, string propertyName)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException($"{propertyName} must be between {minValue} and {maxValue}.");
    }

    public static void ValidateNullablePositive(long? value, string propertyName)
    {
        if (value.HasValue && value.Value <= 0)
            throw new ArgumentException($"{propertyName} must be a positive number.");
    }

    public static void ValidateNullableGreaterThan(long? value, long minValue, string propertyName)
    {
        if (value.HasValue && value.Value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }
    #endregion

    #region Int Validation
    public static void ValidatePositive(int value, string propertyName)
    {
        if (value <= 0)
            throw new ArgumentException($"{propertyName} must be a positive number.");
    }

    public static void ValidatePositiveOrZero(int value, string propertyName)
    {
        if (value < 0)
            throw new ArgumentException($"{propertyName} must be a positive number or zero.");
    }

    public static void ValidateGreaterThan(int value, int minValue, string propertyName)
    {
        if (value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }

    public static void ValidateLessThan(int value, int maxValue, string propertyName)
    {
        if (value >= maxValue)
            throw new ArgumentException($"{propertyName} must be less than {maxValue}.");
    }

    public static void ValidateRange(int value, int minValue, int maxValue, string propertyName)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException($"{propertyName} must be between {minValue} and {maxValue}.");
    }

    public static void ValidateNullablePositive(int? value, string propertyName)
    {
        if (value.HasValue && value.Value <= 0)
            throw new ArgumentException($"{propertyName} must be a positive number.");
    }
    #endregion

    #region Decimal Validation
    public static void ValidatePositive(decimal value, string propertyName)
    {
        if (value <= 0)
            throw new ArgumentException($"{propertyName} must be a positive number.");
    }

    public static void ValidatePositiveOrZero(decimal value, string propertyName)
    {
        if (value < 0)
            throw new ArgumentException($"{propertyName} must be a positive number or zero.");
    }

    public static void ValidateGreaterThan(decimal value, decimal minValue, string propertyName)
    {
        if (value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }

    public static void ValidateLessThan(decimal value, decimal maxValue, string propertyName)
    {
        if (value >= maxValue)
            throw new ArgumentException($"{propertyName} must be less than {maxValue}.");
    }

    public static void ValidateRange(decimal value, decimal minValue, decimal maxValue, string propertyName)
    {
        if (value < minValue || value > maxValue)
            throw new ArgumentException($"{propertyName} must be between {minValue} and {maxValue}.");
    }
    #endregion

    #region DateTime Validation
    public static void ValidateNotNull(DateTime? value, string propertyName)
    {
        if (!value.HasValue)
            throw new ArgumentException($"{propertyName} cannot be null.");
    }

    public static void ValidateNotFuture(DateTime value, string propertyName)
    {
        if (value > DateTime.UtcNow)
            throw new ArgumentException($"{propertyName} cannot be in the future.");
    }

    public static void ValidateNotPast(DateTime value, string propertyName)
    {
        if (value < DateTime.UtcNow)
            throw new ArgumentException($"{propertyName} cannot be in the past.");
    }

    public static void ValidateGreaterThan(DateTime value, DateTime minValue, string propertyName)
    {
        if (value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }

    public static void ValidateLessThan(DateTime value, DateTime maxValue, string propertyName)
    {
        if (value >= maxValue)
            throw new ArgumentException($"{propertyName} must be less than {maxValue}.");
    }

    public static void ValidateNullableNotFuture(DateTime? value, string propertyName)
    {
        if (value.HasValue && value.Value > DateTime.UtcNow)
            throw new ArgumentException($"{propertyName} cannot be in the future.");
    }

    public static void ValidateNullableGreaterThan(DateTime? value, DateTime minValue, string propertyName)
    {
        if (value.HasValue && value.Value <= minValue)
            throw new ArgumentException($"{propertyName} must be greater than {minValue}.");
    }
    #endregion

    #region Object Validation
    public static void ValidateNotNull(object? value, string propertyName)
    {
        if (value is null)
            throw new ArgumentException($"{propertyName} cannot be null.");
    }
    #endregion

    #region Collection Validation
    public static void ValidateNotNull<T>(IReadOnlyCollection<T>? value, string propertyName)
    {
        if (value is null)
            throw new ArgumentException($"{propertyName} cannot be null.");
    }

    public static void ValidateNotEmpty<T>(IReadOnlyCollection<T>? value, string propertyName)
    {
        if (value is not null && value.Count == 0)
            throw new ArgumentException($"{propertyName} cannot be empty.");
    }

    public static void ValidateNotNullOrEmpty<T>(IReadOnlyCollection<T>? value, string propertyName)
    {
        ValidateNotNull(value, propertyName);
        ValidateNotEmpty(value, propertyName);
    }
    #endregion
}
