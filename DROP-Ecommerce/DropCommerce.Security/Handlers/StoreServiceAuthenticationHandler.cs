using Commerce.Security;
using DropCommerce.Application.Integrations.Store.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DropCommerce.Security.Handlers;

/// <summary>
/// Autentica as chamadas internas ao Store e informa a loja para preservar o isolamento dos dados.
/// </summary>
public sealed class StoreServiceAuthenticationHandler(IOptions<SecurityOptions> options, IHttpContextAccessor accessor) : StoreAuthenticationHandler
{
    /// <summary>
    /// Envia a chave de serviço e a loja autenticada ao Store, protegendo as consultas entre os módulos.
    /// </summary>
    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
    {
        if (options.Value.InternalServiceKey.Length < 32) 
            throw new SecurityFailure(503, "Security.ServiceNotConfigured", "Integração de identidade não configurada.");

        var tenant = SecurityNames.Id(accessor.HttpContext?.User ?? throw new SecurityFailure(401, "Security.TenantRequired", "Autenticação necessária."), SecurityNames.Enterprise);

        request.Headers.Add("X-Service-Key", options.Value.InternalServiceKey);
        request.Headers.Add("X-Enterprise-Id", tenant.ToString(System.Globalization.CultureInfo.InvariantCulture));

        return base.SendAsync(request, ct);
    }
}
