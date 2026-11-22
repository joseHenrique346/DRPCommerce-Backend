namespace DropCommerce.Application.Contracts;

public sealed record DropStorefrontProductResponse(
    long ParticipationId,
    string Name,
    string Description,
    int MaxPerCustomer,
    bool ConcentrationWarning,
    IEnumerable<DropStorefrontVariantResponse> Variants);
