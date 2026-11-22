namespace StoreCommerce.Application.Contracts;

public sealed record EnterpriseAddOnResponse(
    long Id,
    string Code,
    string Name,
    string ResourceCode,
    int Quantity,
    decimal MonthlyPrice,
    IReadOnlyList<long> EligiblePlanIds);
