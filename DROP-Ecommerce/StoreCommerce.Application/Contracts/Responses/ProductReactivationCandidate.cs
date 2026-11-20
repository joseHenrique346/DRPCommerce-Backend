namespace StoreCommerce.Application.Contracts;

public sealed record ProductReactivationCandidate(
    long Id,
    string Name,
    string SKU,
    DateTime CreatedAt,
    string? BlockReason);
