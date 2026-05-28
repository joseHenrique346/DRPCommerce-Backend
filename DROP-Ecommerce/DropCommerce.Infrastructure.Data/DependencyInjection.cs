using DropCommerce.Domain.Interfaces;
using DropCommerce.Infrastructure.Data.Context;
using DropCommerce.Infrastructure.Data.Providers;
using DropCommerce.Infrastructure.Data.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DropCommerce.Infrastructure.Data;

public static class DependencyInjection
{
    public static IServiceCollection AddDropInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<DropCommerceDbContext>(options =>
            options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        services.AddScoped<ITenantProvider, TenantProvider>();
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped(typeof(IRepository<>), typeof(Repository<>));

        services.AddScoped<IDropEventRepository, DropEventRepository>();
        services.AddScoped<IDropOrderRepository, DropOrderRepository>();
        services.AddScoped<IDropProductRepository, DropProductRepository>();
        services.AddScoped<IDropCouponRepository, DropCouponRepository>();
        services.AddScoped<IDropRegistrationRepository, DropRegistrationRepository>();
        services.AddScoped<IQueueEntryRepository, QueueEntryRepository>();

        return services;
    }
}
