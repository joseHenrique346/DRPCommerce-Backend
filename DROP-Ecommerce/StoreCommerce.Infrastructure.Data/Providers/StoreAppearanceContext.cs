using System.Security.Claims;
using Commerce.Appearance.Application;
using Commerce.Appearance.Domain;
using Commerce.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Providers;

public sealed class StoreAppearanceContext(IHttpContextAccessor accessor, ITenantProvider tenant, IOptions<SecurityOptions> options) : IAppearanceContext
{
    public long EnterpriseId => tenant.GetEnterpriseId();
    public long UserId => SecurityNames.Id(Principal, "sub");
    private ClaimsPrincipal Principal => accessor.HttpContext?.User ?? new ClaimsPrincipal();

    public void RequireAdministrator()
    {
        if (Principal.Identity?.IsAuthenticated != true)
            throw new AppearanceFailure(401, "Appearance.AuthenticationRequired", "Autenticação necessária.");

        if (EnterpriseId < 1 || Principal.FindFirst(SecurityNames.Actor)?.Value != "user" ||
            Principal.FindFirst(SecurityNames.Kind)?.Value != "primary_owner" || Principal.FindFirst(SecurityNames.Membership) is null)
            throw new AppearanceFailure(403, "Appearance.OwnerRequired", "Apenas o dono principal da loja pode gerenciar a aparência.");
    }

    public void RequireCatalogPublisher()
    {
        var key = options.Value.InternalServiceKey;
        var request = accessor.HttpContext?.Request;

        if (key.Length < 32)
            throw new AppearanceFailure(503, "Appearance.PublisherUnavailable", "Publicação do catálogo não configurada.");

        if (request is null || !request.Path.StartsWithSegments("/api/internal/appearance-themes") ||
            !OpaqueToken.EqualsSecret(key, request.Headers["X-Service-Key"].ToString()))
            throw new AppearanceFailure(401, "Appearance.PublisherRequired", "Autenticação da plataforma necessária.");
    }
}
