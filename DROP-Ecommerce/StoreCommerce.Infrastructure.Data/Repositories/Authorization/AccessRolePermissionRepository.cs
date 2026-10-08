using System.Linq.Expressions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class AccessRolePermissionRepository(StoreCommerceDbContext context) : RelationshipRepository<AccessRolePermission>(context), IAccessRolePermissionRepository
{
    protected override Expression<Func<AccessRolePermission, bool>> MatchKey(long firstId, long secondId) =>
        entity => entity.AccessRoleId == firstId && entity.PermissionId == secondId;
}
