using SharedCommerce.Infrastructure.Data;
using SharedCommerce.Infrastructure.Data.Repositories;
using SharedCommerce.Infrastructure.Data.Context;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Domain.StaticEntity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Infrastructure.Data.Context;
using StoreCommerce.Infrastructure.Data.Providers;
using StoreCommerce.Infrastructure.Data.Repositories;

namespace StoreCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddStoreInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<StoreCommerceDbContext>(options =>
            options.UseNpgsql(connectionString));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddSharedInfrastructure();
        services.AddScoped<IAppearanceContext, StoreAppearanceContext>();
        services.AddScoped<IAppearanceResources, StoreAppearanceResources>();
        services.AddScoped<IAppearanceRepository>(provider => new AppearanceRepository<StoreCommerceDbContext>(
            provider.GetRequiredService<StoreCommerceDbContext>(),
            provider.GetRequiredService<AppearanceWriteScope>(),
            AppearanceChannel.Store));
        services.AddScoped<UnitOfWork>();
        services.AddScoped<IUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        services.AddScoped<IIdentityUnitOfWork>(provider => provider.GetRequiredService<UnitOfWork>());
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));
        services.AddScoped(typeof(IEntityRepository<>), typeof(EntityRepository<>));
        services.AddScoped<IIdentityRepositories, IdentityRepositories>();
        services.AddScoped<IAuthenticationSessionRepository, AuthenticationSessionRepository>();

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IEnterpriseRepository, EnterpriseRepository>();
        services.AddScoped<IEnterpriseMembershipRepository, EnterpriseMembershipRepository>();
        services.AddScoped<IEnterpriseDomainRepository, EnterpriseDomainRepository>();
        services.AddScoped<IAccessRoleRepository, AccessRoleRepository>();
        services.AddScoped<IEmployeePositionRepository, EmployeePositionRepository>();
        services.AddScoped<IPermissionRepository, PermissionRepository>();
        services.AddScoped<IAccessRolePermissionRepository, AccessRolePermissionRepository>();
        services.AddScoped<IMembershipAccessRoleRepository, MembershipAccessRoleRepository>();

        return services;
    }
}
