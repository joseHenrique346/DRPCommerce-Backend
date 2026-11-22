namespace DropCommerce.Application.Contracts;

public sealed record DropQueueStatusResponse(
    long Id,
    int Position,
    long QueueEntryStatusId,
    DropQueueSessionResponse? Session);
