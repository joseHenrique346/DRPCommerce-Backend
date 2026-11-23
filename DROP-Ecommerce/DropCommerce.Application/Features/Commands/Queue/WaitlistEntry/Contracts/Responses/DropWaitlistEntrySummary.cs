namespace DropCommerce.Application.Features.Commands;

public sealed record DropWaitlistEntrySummary(long Id, long DropEventId, long? DropProductId, int Position, long WaitlistEntryStatusId);
