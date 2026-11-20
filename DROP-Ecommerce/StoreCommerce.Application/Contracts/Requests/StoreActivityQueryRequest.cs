namespace StoreCommerce.Application.Contracts;

public sealed record StoreActivityQueryRequest(
    long? ProductId = null,
    string? EventType = null,
    DateTime? FromUtc = null,
    DateTime? ToUtc = null,
    int Page = 1,
    int PageSize = 30);
