namespace StoreCommerce.Application.Contracts;

public sealed record EnterpriseContractChangeResponse(
    long Id,
    long EnterpriseId,
    string Status,
    long PlanId,
    long? AddOnId,
    long ExpectedContractRevision,
    int CatalogVersion,
    string Currency,
    decimal PlanMonthlyPrice,
    decimal AddOnMonthlyPrice,
    decimal TotalMonthlyPrice,
    DateTime RequestedAt,
    DateTime? SupersededAt,
    bool GrantsAccess,
    IReadOnlyList<long>? RetainedMembershipIds);
