using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using DropCommerce.Infrastructure.Data.Integrations.Store.Exceptions;
using DropCommerce.Infrastructure.Data.Integrations.Store.Readers;
using Refit;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class AppearanceIntegrationFailureTests
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Store_connection_failure_never_returns_unverified_rights_or_media(bool media)
    {
        using var client = new HttpClient(new BrokenConnection()) { BaseAddress = new Uri("https://store.example.invalid") };
        var api = RestService.For<IStoreEnterpriseRefitApi>(client);
        var reader = new StoreAppearanceReader(api, new StoreIntegrationScope());

        if (media)
            await Assert.ThrowsAsync<StoreIntegrationException>(() => reader.RetainMediaAsync(1, 1, default));
        else
            await Assert.ThrowsAsync<StoreIntegrationException>(() => reader.RightsAsync(1, default));
    }

    private sealed class BrokenConnection : HttpMessageHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken ct)
        {
            throw new HttpRequestException("Falha simulada na conexão com o Store.");
        }
    }
}
