using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record EnterpriseMembershipResponse
{
    public static EnterpriseMembershipResponse FromEntity(EnterpriseMembership e) => new(e.Id, e.EnterpriseId, e.UserId,
        e.KindId, e.IsActive, e.JoinedAt, e.IsContractBlocked, e.ContractBlockedAt);
}
