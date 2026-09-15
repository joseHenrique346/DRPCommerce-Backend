using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
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
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<ICustomerRepository, CustomerRepository>();
        services.AddScoped<ICouponRepository, CouponRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IServiceRepository, ServiceRepository>();

        return services;
    }
}
