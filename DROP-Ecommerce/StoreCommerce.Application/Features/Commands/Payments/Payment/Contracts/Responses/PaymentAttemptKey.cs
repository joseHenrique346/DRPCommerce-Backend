using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentAttemptKey(long EnterpriseId, long AttemptId);
