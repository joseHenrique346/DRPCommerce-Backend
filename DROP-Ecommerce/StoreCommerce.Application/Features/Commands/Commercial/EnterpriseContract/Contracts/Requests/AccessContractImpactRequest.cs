namespace StoreCommerce.Application.Contracts;

public sealed record AccessContractImpactRequest(
    long PlanId, long? AddOnId, long ExpectedContractRevision, int CatalogVersion,
    IReadOnlyList<long>? RetainedMembershipIds = null);
