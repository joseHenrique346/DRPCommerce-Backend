using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ProviderNotificationSummary(long Id, long ProviderConnectionId, long? PaymentAttemptId, string PaymentId, ProviderNotificationStatus Status, string? ResolutionCode, DateTime CreatedAt);
