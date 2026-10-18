using System.Net.Http.Json;
using System.Text.Json;
using Commerce.Security;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DropCommerce.Security.Resolvers;

/// <summary>
/// Identifica a loja do storefront usando o Store como fonte confiável de domínios.
/// </summary>
public sealed class DropTenantResolver(HttpClient client, IOptions<SecurityOptions> options) : IRequestTenantResolver
{
    /// <summary>
    /// Consulta o Store para identificar a loja pelo domínio, impedindo acesso a um storefront desconhecido.
    /// </summary>
    public async Task ResolveAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/storefront"))
            return;

        if (options.Value.InternalServiceKey.Length < 32)
            throw new SecurityFailure(503, "Security.ServiceNotConfigured", "Resolução de loja indisponível.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/internal/tenants/resolve?host=" + Uri.EscapeDataString(context.Request.Host.Host));

        request.Headers.Add("X-Service-Key", options.Value.InternalServiceKey);

        try
        {
            using var response = await client.SendAsync(request, context.RequestAborted);

            if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

            if (!response.IsSuccessStatusCode)
                throw new SecurityFailure(503, "Security.StoreUnavailable", "Resolução de loja indisponível.");

            var tenant = await response.Content.ReadFromJsonAsync<TenantResponse>(cancellationToken: context.RequestAborted);

            if (tenant is null || tenant.EnterpriseId < 1)
                throw new SecurityFailure(503, "Security.StoreUnavailable", "Resolução de loja indisponível.");

            context.Items[SecurityNames.ResolvedTenant] = tenant.EnterpriseId;
        }
        catch (HttpRequestException)
        {
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Resolução de loja indisponível.");
        }
        catch (JsonException)
        {
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Resposta de loja inválida.");
        }
        catch (OperationCanceledException) when (!context.RequestAborted.IsCancellationRequested)
        {
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Resolução de loja indisponível.");
        }
    }

    /// <summary>
    /// Recebe somente a identificação da loja retornada pelo Store para limitar os dados da integração.
    /// </summary>
    private sealed record TenantResponse(long EnterpriseId);
}
