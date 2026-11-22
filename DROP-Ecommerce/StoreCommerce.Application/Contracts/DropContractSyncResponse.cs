namespace StoreCommerce.Application.Contracts;

public sealed record DropContractSyncResponse(long SourceRevision, long AcknowledgedRevision,
    bool Pending, DateTime ChangedAt, DateTime? AcknowledgedAt);
