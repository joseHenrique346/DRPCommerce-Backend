using System.Net;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Exceptions;

public sealed class StoreIntegrationException : Exception
{
    public HttpStatusCode? StatusCode { get; }

    public StoreIntegrationException(string message, HttpStatusCode? statusCode = null, Exception? innerException = null)
        : base(message, innerException)
    {
        StatusCode = statusCode;
    }
}
