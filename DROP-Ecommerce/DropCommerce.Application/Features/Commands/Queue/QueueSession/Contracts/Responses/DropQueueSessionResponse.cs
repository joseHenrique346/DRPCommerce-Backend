namespace DropCommerce.Application.Contracts;

public sealed record DropQueueSessionResponse(
    long Id,
    DateTime IssuedAt,
    DateTime ExpiresAt);
