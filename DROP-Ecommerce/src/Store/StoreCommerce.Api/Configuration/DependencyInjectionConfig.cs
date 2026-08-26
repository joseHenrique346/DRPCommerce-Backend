using StoreCommerce.Api.ExceptionHandling;
using StoreCommerce.Application;
using StoreCommerce.Infrastructure.Data;

namespace StoreCommerce.Api.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers();
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.AddSwaggerGen();

        var connectionString = Environment.GetEnvironmentVariable("STORECOMMERCE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")!;

        services.AddStoreInfrastructure(connectionString);
        services.AddStoreApplication();

        return services;
    }
}
