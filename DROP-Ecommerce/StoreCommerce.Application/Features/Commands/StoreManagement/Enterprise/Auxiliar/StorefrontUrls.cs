namespace StoreCommerce.Application.Features.Commands;

public static class StorefrontUrls
{
    public static string? Page(string baseUrl, string? slug)
    {
        return slug is null ? null : baseUrl.Trim().TrimEnd('/') + "/" + slug;
    }

    public static string? Media(string apiBaseUrl, string? slug, Guid publicId)
    {
        return slug is null ? null : $"{apiBaseUrl.Trim().TrimEnd('/')}/api/storefront/{slug}/media/{publicId}";
    }
}
