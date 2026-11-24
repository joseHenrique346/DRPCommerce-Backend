using System.Text.RegularExpressions;

namespace SharedCommerce.Domain.Contracts.Storefront;

public static class StorefrontSlug
{
    public const int MaxLength = 63;

    public static string? Normalize(string? value)
    {
        var slug = value?.Trim().ToLowerInvariant();

        if (slug is null || !Regex.IsMatch(slug, "^[a-z0-9][a-z0-9-]{1,61}[a-z0-9]$", RegexOptions.CultureInvariant) ||
            slug is "www" or "api" or "admin" or "backoffice" or "mail" or "internal" or "swagger" or "openapi")
            return null;

        return slug;
    }
}
