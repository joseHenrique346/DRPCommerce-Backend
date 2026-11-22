namespace DropCommerce.Application.Contracts;

public sealed record DropReactivationResponse(
    long ContractRevision,
    int Available,
    IReadOnlyList<long> ParticipationIds,
    bool Applied);
