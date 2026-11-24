using StoreCommerce.Domain.Interfaces.StoreManagement;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseDomainRepository(StoreCommerceDbContext context) : Repository<EnterpriseDomain>(context), IEnterpriseDomainRepository;
