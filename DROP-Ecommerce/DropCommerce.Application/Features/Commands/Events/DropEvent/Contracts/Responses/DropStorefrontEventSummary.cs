namespace DropCommerce.Application.Contracts;

public sealed record DropStorefrontEventSummary(
    long Id,
    string Name,
    string Slug,
    string Description,
    decimal Price,
    string CoverImageUrl,
    DateTime DropStartsAt,
    DateTime DropEndsAt,
    bool RequiresRegistration);
