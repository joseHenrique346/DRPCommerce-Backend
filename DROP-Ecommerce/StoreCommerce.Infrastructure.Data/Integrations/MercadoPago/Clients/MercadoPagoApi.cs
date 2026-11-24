using StoreCommerce.Domain.Enums.Integrations;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoApi(IHttpClientFactory clients)
{
    public async Task<JsonDocument> OAuthAsync(object body, CancellationToken ct)
    {
        try
        {
            return await SendAsync(HttpMethod.Post, "/oauth/token", null, body, null, ct);
        }
        catch (MercadoPagoApiException exception) when (exception.Status is HttpStatusCode.BadRequest or HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            throw new SecurityFailure(409, "Integration.ReauthorizationRequired", "O provedor recusou a autorização. Conecte novamente a conta.");
        }
    }

    public async Task<MercadoPagoAccount> AccountAsync(string token, CancellationToken ct)
    {
        using var json = await SendAsync(HttpMethod.Get, "/users/me", token, null, null, ct);
        var root = json.RootElement;
        var tags = root.TryGetProperty("tags", out var values) && values.ValueKind == JsonValueKind.Array
            ? values.EnumerateArray().Select(Value => Value.GetString()).ToArray() : [];

        return new MercadoPagoAccount(Id(root, "id"), tags.Contains("test_user"), Text(root, "site_id"));
    }

    public static void ValidateAccount(MercadoPagoAccount account, IntegrationEnvironment environment)
    {
        if (account.SiteId != "MLB" || account.IsTestUser != (environment == IntegrationEnvironment.Sandbox))
            throw new SecurityFailure(409, "MercadoPago.EnvironmentMismatch",
                "Use vendedor de teste no sandbox e uma conta brasileira real em produção.");
    }

    public async Task<JsonDocument> SendAsync(HttpMethod method, string path, string? accessToken,
        object? body, string? idempotencyKey, CancellationToken ct)
    {
        if (!path.StartsWith('/') || path.StartsWith("//") || path.Contains("://"))
            throw new ArgumentException("Caminho inválido da API.");

        using var request = new HttpRequestMessage(method, path);

        if (accessToken is not null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

        if (idempotencyKey is not null)
            request.Headers.Add("X-Idempotency-Key", idempotencyKey);

        if (body is not null)
            request.Content = JsonContent.Create(body);

        using var response = await clients.CreateClient("MercadoPago").SendAsync(request, HttpCompletionOption.ResponseHeadersRead, ct);

        if (!response.IsSuccessStatusCode)
            throw new MercadoPagoApiException(response.StatusCode);

        await using var stream = await response.Content.ReadAsStreamAsync(ct);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int read;

        while ((read = await stream.ReadAsync(chunk, ct)) > 0)
        {
            if (buffer.Length + read > 2_000_000)
                throw new SecurityFailure(502, "MercadoPago.InvalidResponse", "Resposta do provedor excedeu o limite.");

            await buffer.WriteAsync(chunk.AsMemory(0, read), ct);
        }

        return JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 64 });
    }

    public static string Text(JsonElement element, string name)
    {
        return element.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.String
            ? value.GetString() ?? string.Empty : string.Empty;
    }

    public static string Id(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind is not (JsonValueKind.String or JsonValueKind.Number))
            throw new SecurityFailure(502, "MercadoPago.InvalidResponse", "Identificador ausente na resposta do provedor.");

        var id = value.ValueKind == JsonValueKind.String ? value.GetString()! : value.GetRawText();

        if (string.IsNullOrWhiteSpace(id) || id.Length > 200)
            throw new SecurityFailure(502, "MercadoPago.InvalidResponse", "Identificador inválido do provedor.");

        return id;
    }

    public static decimal Money(JsonElement element, string name)
    {
        if (!element.TryGetProperty(name, out var value) || value.ValueKind != JsonValueKind.Number ||
            !value.TryGetDecimal(out var amount) || amount < 0)
            throw new SecurityFailure(502, "MercadoPago.InvalidResponse", "Valor inválido na resposta do provedor.");

        return amount;
    }
}
