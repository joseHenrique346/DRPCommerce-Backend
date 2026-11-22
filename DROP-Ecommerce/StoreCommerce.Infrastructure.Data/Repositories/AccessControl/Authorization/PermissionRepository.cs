using StoreCommerce.Domain.Interfaces.AccessControl;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class PermissionRepository(StoreCommerceDbContext context) : Repository<Permission>(context), IPermissionRepository;
