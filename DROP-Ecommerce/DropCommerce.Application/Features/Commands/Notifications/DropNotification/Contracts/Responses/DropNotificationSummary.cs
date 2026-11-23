namespace DropCommerce.Application.Features.Commands;

public sealed record DropNotificationSummary(long Id, string Subject, string Body, DateTime? ScheduledAt);
