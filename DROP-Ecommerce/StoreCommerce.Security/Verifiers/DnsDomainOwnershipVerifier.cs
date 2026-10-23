using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using System.Text.Json;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Security.Verifiers;

/// <summary>
/// Comprova o controle do domínio por DNS antes de permitir sua publicação em uma loja.
/// </summary>
public sealed class DnsDomainOwnershipVerifier(HttpClient client) : IDomainOwnershipVerifier
{
    /// <summary>
    /// Confere o desafio TXT no DNS para comprovar o controle do domínio antes de vinculá-lo à loja.
    /// </summary>
    public async Task<bool> VerifyAsync(string host, string expectedHash, CancellationToken ct)
    {
        var queryName = "_drpcommerce." + HostName.Normalize(host);

        try
        {
            using var response = await client.GetAsync("https://dns.google/resolve?type=TXT&edns_client_subnet=0.0.0.0%2F0&name=" + Uri.EscapeDataString(queryName), ct);

            if (!response.IsSuccessStatusCode)
                throw new SecurityFailure(503, "Domain.DnsUnavailable", "Verificação DNS indisponível.");

            using var document = await JsonDocument.ParseAsync(await response.Content.ReadAsStreamAsync(ct), cancellationToken: ct);
            var root = document.RootElement;

            if (!root.TryGetProperty("Status", out var status) || status.GetInt32() != 0 || !root.TryGetProperty("Answer", out var answers))
                return false;

            foreach (var answer in answers.EnumerateArray())
            {
                if (answer.GetProperty("type").GetInt32() != 16)
                    continue;

                var value = answer.GetProperty("data").GetString()?.Trim('"') ?? "";
                const string prefix = "drpcommerce-verification=";

                if (value.StartsWith(prefix, StringComparison.Ordinal) && OpaqueToken.EqualsSecret(expectedHash, OpaqueToken.Hash(value[prefix.Length..])))
                    return true;
            }

            return false;
        }
        catch (HttpRequestException)
        {
            throw new SecurityFailure(503, "Domain.DnsUnavailable", "Verificação DNS indisponível.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new SecurityFailure(503, "Domain.DnsUnavailable", "Verificação DNS indisponível.");
        }
        catch (JsonException)
        {
            throw new SecurityFailure(503, "Domain.DnsUnavailable", "Resposta DNS inválida.");
        }
    }
}
