namespace DropCommerce.Application.Contracts;

public sealed record ReactivateDropParticipations(long ExpectedContractRevision, string Mode, IReadOnlyList<long>? ParticipationIds);
