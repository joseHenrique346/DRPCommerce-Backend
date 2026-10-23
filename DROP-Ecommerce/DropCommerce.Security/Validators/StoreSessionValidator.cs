using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Options;
using SharedCommerce.Application.Interfaces;
using System.Net.Http.Json;
using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;

namespace DropCommerce.Security.Validators;

/// <summary>
/// Consulta o Store para que o Drop também reconheça sessões revogadas e mudanças de acesso.
/// </summary>
public sealed class StoreSessionValidator(HttpClient client, IHttpContextAccessor accessor, IOptions<SecurityOptions> options) : ISessionValidator
{
    /// <summary>
    /// Confirma a sessão no Store para rejeitar acessos revogados, expirados ou cuja validação esteja indisponível.
    /// </summary>
    public async Task<bool> ValidateAsync(ClaimsPrincipal principal, string? host, CancellationToken ct)
    {
        var header = accessor.HttpContext?.Request.Headers.Authorization.ToString() ?? "";

        if (!header.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) 
            return false;

        if (options.Value.InternalServiceKey.Length < 32) 
            throw new SecurityFailure(503, "Security.ServiceNotConfigured", "Validação de identidade indisponível.");

        using var request = new HttpRequestMessage(HttpMethod.Post, "api/internal/identity/validate")
        {
            Content = JsonContent.Create(new { accessToken = header[7..], host = host ?? "" })
        };

        request.Headers.Add("X-Service-Key", options.Value.InternalServiceKey);

        try
        {
            using var response = await client.SendAsync(request, ct);

            if (!response.IsSuccessStatusCode) 
                throw new SecurityFailure(503, "Security.StoreUnavailable", "Validação de identidade indisponível.");

            var result = await response.Content.ReadFromJsonAsync<ValidationResponse>(cancellationToken: ct);
            return result?.IsValid == true;
        }
        catch (HttpRequestException) 
        { 
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Validação de identidade indisponível."); 
        }
        catch (System.Text.Json.JsonException) 
        { 
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Resposta de identidade inválida."); 
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested) 
        { 
            throw new SecurityFailure(503, "Security.StoreUnavailable", "Validação de identidade indisponível."); 
        }
    }
    /// <summary>
    /// Recebe o resultado da validação sem transportar dados sensíveis da conta entre os módulos.
    /// </summary>
    private sealed record ValidationResponse(bool IsValid);
}
