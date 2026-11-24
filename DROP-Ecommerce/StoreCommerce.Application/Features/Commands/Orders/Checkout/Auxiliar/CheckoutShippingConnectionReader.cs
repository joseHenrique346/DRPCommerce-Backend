using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CheckoutShippingConnectionReader(IProviderConnectionStore connections, IOptions<MelhorEnvioOptions> options)
{
    public async Task<ProviderConnection?> FindAsync(long enterpriseId, CancellationToken ct)
    {
        var settings = options.Value;

        if (settings.ShippingEnvironment is not { } environment)
            return null;

        var connection = await connections.GetAsync(enterpriseId, IntegrationProviders.MelhorEnvio, environment, ct);

        if (connection is not { IsConnected: true })
            return null;

        settings.RequireOAuth(environment);

        return connection;
    }
}
