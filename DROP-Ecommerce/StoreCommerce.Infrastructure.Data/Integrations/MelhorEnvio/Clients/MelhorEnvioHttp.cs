using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;

internal static class MelhorEnvioHttp
{
    public static string Origin(IntegrationEnvironment environment)
    {
        return environment switch
        {
            IntegrationEnvironment.Sandbox => "https://sandbox.melhorenvio.com.br",
            IntegrationEnvironment.Production => "https://melhorenvio.com.br",
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };
    }

    public static HttpRequestMessage Request(HttpMethod method, IntegrationEnvironment environment, string path, string contactEmail)
    {
        var request = new HttpRequestMessage(method, Origin(environment) + path);
        request.Headers.Accept.ParseAdd("application/json");
        request.Headers.UserAgent.ParseAdd($"DRPCommerce ({contactEmail})");

        return request;
    }

    public static async Task<HttpResponseMessage> SendAsync(HttpClient http, HttpRequestMessage request, CancellationToken ct)
    {
        try
        {
            return await http.SendAsync(request, ct);
        }
        catch (HttpRequestException)
        {
            throw new SecurityFailure(502, "MelhorEnvio.Unavailable", "O Melhor Envio está indisponível.");
        }
        catch (OperationCanceledException) when (!ct.IsCancellationRequested)
        {
            throw new SecurityFailure(504, "MelhorEnvio.Timeout", "O Melhor Envio não respondeu a tempo.");
        }
    }
}
