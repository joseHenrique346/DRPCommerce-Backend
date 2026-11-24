using SharedCommerce.Application.Interfaces.Tenancy;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using SharedCommerce.Application.Options;
using SharedCommerce.Domain.Contracts.Storefront;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DropCommerce.Security.Resolvers;

/// <summary>
/// Identifica a loja do storefront usando o Store como fonte confiável de slugs.
/// </summary>
public sealed class DropTenantResolver(HttpClient client, IOptions<SecurityOptions> options) : IRequestTenantResolver
{
    /// <summary>
    /// Consulta o Store para identificar a loja pelo slug da rota.
    /// </summary>
    public async Task ResolveAsync(HttpContext context)
    {
        if (!context.Request.Path.StartsWithSegments("/api/storefront"))
            return;

        var slug = StorefrontSlug.Normalize(context.Request.RouteValues["storeSlug"] as string);

        if (slug is null)
            throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (options.Value.InternalServiceKey.Length < 32)
            throw new SecurityFailure(503, "Security.ServiceNotConfigured", "Resolução de loja indisponível.");

        using var request = new HttpRequestMessage(HttpMethod.Get, "api/internal/tenants/resolve?slug=" + Uri.EscapeDataString(slug));

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
