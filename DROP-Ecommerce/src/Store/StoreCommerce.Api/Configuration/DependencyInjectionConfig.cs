using Microsoft.OpenApi.Models;
using StoreCommerce.Api.Configuration.Swagger;
using StoreCommerce.Api.ExceptionHandling;
using StoreCommerce.Application;
using StoreCommerce.Infrastructure.Data;
using StoreCommerce.Security;
using StoreCommerce.Api.Security;

namespace StoreCommerce.Api.Configuration;

public static class DependencyInjectionConfig
{
    public static IServiceCollection AddApiConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHttpContextAccessor();
        services.AddControllers(options => options.Filters.Add<StoreOperationFilter>())
            .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles);
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        services.AddOpenApi();
        services.AddSwaggerGen(options =>
        {
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "StoreCommerce.Api.xml"));
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "SharedCommerce.Api.xml"));

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
                Description = "Cole somente o accessToken retornado pelo login, sem o prefixo Bearer. Cadastro, login e obtenção de CSRF não exigem esse token."
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

            options.OperationFilter<CsrfHeaderOperationFilter>();
        });

        var connectionString = Environment.GetEnvironmentVariable("STORECOMMERCE_CONNECTION_STRING");

        if (string.IsNullOrWhiteSpace(connectionString))
            connectionString = configuration.GetConnectionString("DefaultConnection");

        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException("Configure STORECOMMERCE_CONNECTION_STRING ou ConnectionStrings:DefaultConnection.");

        services.AddStoreInfrastructure(connectionString);
        services.AddStoreApplication();
        services.AddStoreSecurity(configuration);

        return services;
    }
}
