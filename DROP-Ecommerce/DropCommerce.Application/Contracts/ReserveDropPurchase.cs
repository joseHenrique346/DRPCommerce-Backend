namespace DropCommerce.Application.Contracts;

public sealed record ReserveDropPurchase(long ParticipationId, long SessionId, string IdempotencyKey, IReadOnlyList<ReserveDropVariant> Lines);
