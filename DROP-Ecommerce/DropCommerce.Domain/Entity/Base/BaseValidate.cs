namespace DropCommerce.Domain.Entity;

public static class BaseValidate<TProperty>
{
    public static TProperty ValidateNotNullValue(TProperty property)
    {
        if (property == null)
            throw new ArgumentNullException(nameof(property) + " Não pode ser nulo");

        return property;
    }

    public static long ValidateIdValue(long property)
    {
        if (property == 0)
            throw new ArgumentNullException(nameof(property) + " Não pode ser 0");

        return property;
    }

    public static long? ValidateIdValue(long? property)
    {
        if (!property.HasValue)
            return property;

        if (property.Value == 0)
            throw new ArgumentNullException(nameof(property) + " Não pode ser 0");

        return property;
    }

    public static string ValidateStringWhiteSpaceValue(string property)
    {
        if (string.IsNullOrWhiteSpace(property))
            throw new ArgumentNullException(nameof(property) + " Não pode ser vazia");

        return property;
    }
}