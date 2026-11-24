using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Domain.Interfaces.AccessControl;

public interface IMembershipAccessRoleRepository : IRelationshipRepository<MembershipAccessRole> { }
