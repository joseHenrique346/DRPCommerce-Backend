using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record ExternalPaymentOwner(long EnterpriseId, long OrderId, long AttemptId);
