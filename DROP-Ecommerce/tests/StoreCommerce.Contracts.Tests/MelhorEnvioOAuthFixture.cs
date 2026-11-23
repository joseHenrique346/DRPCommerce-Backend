using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Infrastructure.Data.Integrations.MelhorEnvio;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class MelhorEnvioOAuthFixture : IAsyncLifetime
{
    internal MelhorEnvioOAuthHttpStub Provider { get; } = new();
    public ContractsApiFixture Store { get; }

    public MelhorEnvioOAuthFixture()
    {
        Store = new ContractsApiFixture
        {
            ConfigureServices = services =>
            {
                services.AddHttpClient<MelhorEnvioAuthorizationClient>().ConfigurePrimaryHttpMessageHandler(() => Provider);
                services.Configure<MelhorEnvioOptions>(Options =>
                {
                    Options.PublicApiBaseUrl = "https://api.example.invalid";
                    Options.BackofficeReturnUrl = "https://backoffice.example.invalid/integrations";
                    Options.ContactEmail = "integrations@example.invalid";
                    Options.Sandbox.Enabled = true;
                    Options.Sandbox.ClientId = 123;
                    Options.Sandbox.ClientSecret = "test-only-client-secret";
                });
                services.Configure<ProviderIntegrationOptions>(Options => Options.KeyRingPath = "ephemeral-tests-only");
            }
        };
    }

    public Task InitializeAsync()
    {
        return Store.InitializeAsync();
    }

    public Task DisposeAsync()
    {
        return Store.DisposeAsync();
    }
}
