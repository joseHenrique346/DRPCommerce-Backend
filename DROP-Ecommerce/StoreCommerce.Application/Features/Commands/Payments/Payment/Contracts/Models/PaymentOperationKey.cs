using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentOperationKey(long EnterpriseId, long OperationId);
