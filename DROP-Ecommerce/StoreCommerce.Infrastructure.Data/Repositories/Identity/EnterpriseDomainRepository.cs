using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public class EnterpriseDomainRepository(StoreCommerceDbContext context) : Repository<EnterpriseDomain>(context), IEnterpriseDomainRepository;
