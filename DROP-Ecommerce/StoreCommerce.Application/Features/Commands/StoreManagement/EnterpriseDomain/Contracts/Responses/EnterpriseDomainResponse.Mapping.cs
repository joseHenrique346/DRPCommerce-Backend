using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record EnterpriseDomainResponse
{
    public static EnterpriseDomainResponse FromEntity(EnterpriseDomain e) => new(e.Id, e.EnterpriseId, e.Host,
        e.NormalizedHost, e.IsPrimary, e.IsVerified, e.IsActive, e.IsContractBlocked);
}
