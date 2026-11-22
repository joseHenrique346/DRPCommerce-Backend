namespace DropCommerce.Application.Contracts;

public sealed record DropStorefrontEventResponse(
    long Id,
    string Name,
    string Slug,
    string Description,
    string CoverImageUrl,
    string BannerImageUrl,
    bool RequiresRegistration,
    DateTime RegistrationStartsAt,
    DateTime RegistrationEndsAt,
    DateTime QueueOpensAt,
    DateTime DropStartsAt,
    DateTime DropEndsAt,
    int PurchaseWindowMinutes,
    IEnumerable<DropStorefrontProductResponse> Products);
