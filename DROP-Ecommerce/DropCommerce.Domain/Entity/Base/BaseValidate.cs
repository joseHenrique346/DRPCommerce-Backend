using System.Text.RegularExpressions;

namespace DropCommerce.Domain.Entity;

public static class BaseValidate
{
    public static string ValidateRegexString(string value, string pattern, string fieldName)
    {
        ValidateString(value, fieldName);
        if (!Regex.IsMatch(value, pattern))
            throw new ArgumentException($"{fieldName} possui formato inválido.", fieldName);
        return value;
    }

    public static long ValidateId(long value, string fieldName)
    {
        if (value <= 0)
            throw new ArgumentException($"{fieldName} deve ser maior que zero.", fieldName);
        return value;
    }

    public static long? ValidateIdNullable(long? value, string fieldName)
    {
        if (!value.HasValue)
            return value;
        if (value.Value <= 0)
            throw new ArgumentException($"{fieldName} deve ser maior que zero.", fieldName);
        return value;
    }

    public static string ValidateString(string value, string fieldName)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} não pode ser vazia.", fieldName);
        return value;
    }

    public static string? ValidateStringNullable(string? value, string fieldName)
    {
        if (value is not null && string.IsNullOrWhiteSpace(value))
            throw new ArgumentException($"{fieldName} não pode ser apenas espaços em branco.", fieldName);
        return value;
    }

    public static int ValidatePositive(int value, string fieldName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(fieldName, $"{fieldName} não pode ser negativo.");
        return value;
    }

    public static int ValidateMinimum(int value, int min, string fieldName)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(fieldName, $"{fieldName} deve ser no mínimo {min}.");
        return value;
    }

    public static decimal ValidatePositiveDecimal(decimal value, string fieldName)
    {
        if (value < 0)
            throw new ArgumentOutOfRangeException(fieldName, $"{fieldName} não pode ser negativo.");
        return value;
    }

    public static decimal ValidateMinimumDecimal(decimal value, decimal min, string fieldName)
    {
        if (value < min)
            throw new ArgumentOutOfRangeException(fieldName, $"{fieldName} deve ser no mínimo {min}.");
        return value;
    }

    public static DateTime ValidateDate(DateTime value, string fieldName)
    {
        if (value == default)
            throw new ArgumentException($"{fieldName} deve ser uma data válida.", fieldName);
        return value;
    }

    public static void ValidateDateRange(DateTime start, DateTime end, string startField, string endField)
    {
        if (end <= start)
            throw new ArgumentException($"{endField} deve ser posterior a {startField}.");
    }
}
