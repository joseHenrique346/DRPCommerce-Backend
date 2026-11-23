using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentOperationWork(PaymentOperation Operation, Guid LeaseToken, bool RecoverOnly);
