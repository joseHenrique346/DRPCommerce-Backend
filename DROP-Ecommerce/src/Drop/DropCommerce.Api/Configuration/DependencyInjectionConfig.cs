using DropCommerce.Application;
using DropCommerce.Infrastructure.Data;

namespace DropCommerce.Api.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddOpenApi();

        var connectionString = Environment.GetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")!;

        services.AddDropInfrastructure(connectionString);
        services.AddDropApplication();

        return services;
    }
}
