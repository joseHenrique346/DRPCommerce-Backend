using System.Text.Json;

namespace StoreCommerce.Application.Contracts;

public sealed record StoreActivityResponse(
    long Id,
    long? ProductId,
    long? ProductVariantId,
    string EventType,
    string ProductName,
    string SKU,
    string ActorType,
    long? ActorUserId,
    string ReasonCode,
    string CorrelationId,
    DateTime OccurredAt,
    JsonElement Details);
