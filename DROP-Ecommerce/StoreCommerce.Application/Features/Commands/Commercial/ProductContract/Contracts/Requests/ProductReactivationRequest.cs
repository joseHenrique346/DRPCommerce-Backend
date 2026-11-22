namespace StoreCommerce.Application.Contracts;

public sealed record ProductReactivationRequest(
    long ExpectedContractRevision,
    string Mode,
    IReadOnlyList<long>? ProductIds = null);
