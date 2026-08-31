using DropCommerce.Api.ExceptionHandling;
using DropCommerce.Application;
using DropCommerce.Infrastructure.Data;

namespace DropCommerce.Api.Configuration;

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

        var connectionString = Environment.GetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING")
            ?? configuration.GetConnectionString("DefaultConnection")!;

        var storeApiBaseUrl = Environment.GetEnvironmentVariable("STORECOMMERCE_API_BASE_URL")
            ?? configuration["StoreApi:BaseUrl"];

        if (string.IsNullOrWhiteSpace(storeApiBaseUrl))
            throw new InvalidOperationException("A configuração StoreApi:BaseUrl é obrigatória para a integração com o Store.");

        services.AddDropInfrastructure(connectionString, storeApiBaseUrl);
        services.AddDropApplication();

        return services;
    }
}
