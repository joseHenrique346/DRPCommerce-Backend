using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Identity;

public sealed partial record AccessRoleResponse
{
    public static AccessRoleResponse FromEntity(AccessRole e) => new(e.Id, e.EnterpriseId, e.Name, e.NormalizedName, e.Description, e.IsActive);
}
