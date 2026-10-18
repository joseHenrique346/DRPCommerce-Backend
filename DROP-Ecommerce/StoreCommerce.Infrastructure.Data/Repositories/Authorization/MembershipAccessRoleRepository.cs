using System.Linq.Expressions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class MembershipAccessRoleRepository(StoreCommerceDbContext context) : RelationshipRepository<MembershipAccessRole>(context), IMembershipAccessRoleRepository
{
    protected override Expression<Func<MembershipAccessRole, bool>> MatchKey(long firstId, long secondId) =>
        entity => entity.EnterpriseMembershipId == firstId && entity.AccessRoleId == secondId;
}
