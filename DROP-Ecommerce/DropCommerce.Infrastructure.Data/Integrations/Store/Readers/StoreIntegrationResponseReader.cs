using System.Net;
using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using DropCommerce.Infrastructure.Data.Integrations.Store.Exceptions;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

internal static class StoreIntegrationResponseReader
{
    public static T? ReadContentOrNull<T>(ApiResponse<StoreApiResponse<T>> response, string resource)
    {
        if (response.StatusCode == HttpStatusCode.NotFound)
            return default;

        if (!response.IsSuccessStatusCode)
        {
            var statusCode = response.StatusCode;
            var numericStatusCode = statusCode.HasValue ? (int)statusCode.Value : 0;
            throw new StoreIntegrationException($"O Store retornou HTTP {numericStatusCode} ao consultar {resource}.", statusCode, response.Error);
        }

        if (response.Error is not null)
            throw new StoreIntegrationException($"Não foi possível interpretar a resposta do Store ao consultar {resource}.", response.StatusCode, response.Error);

        if (response.Content is null)
            throw new StoreIntegrationException($"O Store retornou uma resposta vazia ao consultar {resource}.", response.StatusCode);

        if (!response.Content.IsSuccess)
        {
            var message = response.Content.ListMessageErrors is { Count: > 0 }
                ? string.Join("; ", response.Content.ListMessageErrors)
                : $"O Store recusou a consulta de {resource}.";

            throw new StoreIntegrationException(message, response.StatusCode);
        }

        if (response.Content.Content is null)
            throw new StoreIntegrationException($"O Store não retornou dados para {resource}.", response.StatusCode);

        return response.Content.Content;
    }

    public static StoreIntegrationException ToIntegrationException(string resource, ApiException exception) =>
        new($"Falha HTTP ao consultar {resource} no Store.", exception.StatusCode, exception);

    public static StoreIntegrationException ToIntegrationException(string resource, HttpRequestException exception) =>
        new($"Não foi possível conectar ao Store para consultar {resource}.", null, exception);

    public static StoreIntegrationException ToIntegrationException(string resource, Exception exception) =>
        new($"Falha na integração com o Store ao consultar {resource}.", null, exception);

    public static StoreIntegrationException ToTimeoutException(string resource, Exception exception) =>
        new($"A consulta de {resource} no Store excedeu o tempo limite.", null, exception);
}
