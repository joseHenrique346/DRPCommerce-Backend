namespace DropCommerce.Application.Contracts;

public sealed record DropQueueEntryResponse(
    long Id,
    long DropEventId,
    int Position,
    long QueueEntryStatusId,
    DateTime EnteredAt);
