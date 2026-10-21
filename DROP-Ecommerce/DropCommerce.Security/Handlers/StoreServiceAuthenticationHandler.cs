using Commerce.Security;
using DropCommerce.Application.Integrations.Store.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DropCommerce.Security.Handlers;

/// <summary>
/// Autentica as chamadas internas ao Store e informa a loja para preservar o isolamento dos dados.
/// </summary>
public sealed class StoreServiceAuthenticationHandler(
    IOptions<SecurityOptions> options,
    IHttpContextAccessor accessor,
    StoreIntegrationScope integration) : StoreAuthenticationHandler
{
    /// <summary>
    /// Envia a chave de serviço e a loja autenticada ao Store, protegendo as consultas entre os módulos.
    /// </summary>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (options.Value.InternalServiceKey.Length < 32) 
            throw new SecurityFailure(503, "Security.ServiceNotConfigured", "Integração de identidade não configurada.");

        var principal = accessor.HttpContext?.User;
        var tokenTenant = principal?.Identity?.IsAuthenticated == true
            ? SecurityNames.Id(principal, SecurityNames.Enterprise)
            : (long?)null;
        var tenant = tokenTenant ?? integration.EnterpriseId
            ?? throw new SecurityFailure(401, "Security.TenantRequired", "Contexto interno ou autenticado de loja necessário.");

        if (integration.EnterpriseId.HasValue && tokenTenant.HasValue && integration.EnterpriseId != tokenTenant)
            throw new SecurityFailure(403, "Security.TenantMismatch", "A operação interna não corresponde à loja autenticada.");

        request.Headers.Add("X-Service-Key", options.Value.InternalServiceKey);
        request.Headers.Add("X-Enterprise-Id", tenant.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return base.SendAsync(request, ct);
    }
}
