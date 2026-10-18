using Microsoft.OpenApi.Models;
using DropCommerce.Api.ExceptionHandling;
using DropCommerce.Application;
using DropCommerce.Infrastructure.Data;
using DropCommerce.Security;
using DropCommerce.Api.Security;
using System.Text.Json.Serialization;

namespace DropCommerce.Api.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers(options => options.Filters.Add<DropOperationFilter>())
            .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles);
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "DropCommerce.Api.xml"));

            options.AddServer(new OpenApiServer
            {
                Url = "../..",
                Description = "API no mesmo host e prefixo utilizados para acessar o Swagger."
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Cole somente o accessToken retornado pelo login no Store, sem o prefixo Bearer. Use um token administrativo ou de cliente conforme a operação."
            });

            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    Array.Empty<string>()
                }
            });
        });

        var connectionString = Environment.GetEnvironmentVariable("DROPCOMMERCE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure DROPCOMMERCE_CONNECTION_STRING ou ConnectionStrings:DefaultConnection.");

        var storeApiBaseUrl = Environment.GetEnvironmentVariable("STORECOMMERCE_API_BASE_URL")
            ?? configuration["StoreApi:BaseUrl"];

        if (string.IsNullOrWhiteSpace(storeApiBaseUrl))
            throw new InvalidOperationException("A configuração StoreApi:BaseUrl é obrigatória para a integração com o Store.");

        services.AddDropInfrastructure(connectionString, storeApiBaseUrl);
        services.AddDropApplication();
        services.AddDropSecurity(configuration);

        return services;
    }
}
