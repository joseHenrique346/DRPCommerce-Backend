using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Contracts;

public sealed record EnterprisePlanResponse(
    long Id,
    string Code,
    string Name,
    decimal MonthlyPrice,
    EnterpriseEntitlements Entitlements);
