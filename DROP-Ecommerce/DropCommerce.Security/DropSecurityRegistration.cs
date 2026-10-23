using SharedCommerce.Api.Configuration;
using SharedCommerce.Application.Options;
using SharedCommerce.Application.Interfaces;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Application.Security;
using DropCommerce.Security.Handlers;
using DropCommerce.Security.Resolvers;
using DropCommerce.Security.Scopes;
using DropCommerce.Security.Validators;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace DropCommerce.Security;

/// <summary>
/// Reúne o registro das proteções do Drop e das dependências de autenticação no Store.
/// </summary>
public static class DropSecurityRegistration
{
    /// <summary>
    /// Registra a segurança do Drop e a comunicação autenticada com o Store, mantendo sessões e lojas validadas.
    /// </summary>
    public static IServiceCollection AddDropSecurity(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddCommerceSecurity(configuration);
        services.AddOptions<SecurityOptions>().Validate(o => o.InternalServiceKey.Length >= 32, "Security:InternalServiceKey é obrigatório no Drop.");

        var baseUrl = Environment.GetEnvironmentVariable("STORECOMMERCE_API_BASE_URL") ?? configuration["StoreApi:BaseUrl"];

        if (!Uri.TryCreate(baseUrl, UriKind.Absolute, out var uri) || uri.Scheme is not ("http" or "https"))
            throw new InvalidOperationException("URL do Store inválida.");

        services.AddHttpClient<ISessionValidator, StoreSessionValidator>(client =>
        {
            client.BaseAddress = uri;
            client.Timeout = TimeSpan.FromSeconds(5);
        });

        services.AddTransient<StoreAuthenticationHandler, StoreServiceAuthenticationHandler>();
        services.AddHttpClient<IRequestTenantResolver, DropTenantResolver>(client =>
        {
            client.BaseAddress = uri;
            client.Timeout = TimeSpan.FromSeconds(5);
        });
        services.AddScoped<IDropOperationScope, DropOperationScope>();

        return services;
    }
}

