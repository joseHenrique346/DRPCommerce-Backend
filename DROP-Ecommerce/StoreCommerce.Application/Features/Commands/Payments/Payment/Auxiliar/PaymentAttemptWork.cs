using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Payments;

public sealed record PaymentAttemptWork(PaymentAttempt Attempt, Guid LeaseToken, bool RecoverOnly);
