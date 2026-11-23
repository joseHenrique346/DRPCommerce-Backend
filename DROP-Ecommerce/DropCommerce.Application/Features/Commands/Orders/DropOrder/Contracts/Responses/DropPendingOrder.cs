namespace DropCommerce.Application.Features.Commands;

public sealed record DropPendingOrder(long Id, long EnterpriseId, bool PaymentIntentAccepted);
