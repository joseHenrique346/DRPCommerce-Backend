namespace StoreCommerce.Application.Contracts;

public sealed record RequestEnterpriseContractChangeRequest(string IdempotencyKey, long PlanId, long? AddOnId, long ExpectedContractRevision, int CatalogVersion, IReadOnlyList<long>? RetainedMembershipIds = null);
