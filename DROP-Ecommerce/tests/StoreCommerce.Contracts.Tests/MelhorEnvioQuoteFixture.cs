using Microsoft.Extensions.DependencyInjection;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Domain.Enums.Integrations;
using Xunit;

namespace StoreCommerce.Contracts.Tests;

public sealed class MelhorEnvioQuoteFixture : IAsyncLifetime
{
    internal MelhorEnvioQuoteHttpStub Provider { get; } = new();
    internal ShippingTestClock Clock { get; } = new();
    public ContractsApiFixture Store { get; }

    public MelhorEnvioQuoteFixture()
    {
        Store = new ContractsApiFixture
        {
            ConfigureServices = services =>
            {
                services.AddHttpClient(nameof(IMelhorEnvioQuoteClient)).ConfigurePrimaryHttpMessageHandler(() => Provider);
                services.Configure<MelhorEnvioOptions>(Options =>
                {
                    Options.ShippingEnvironment = IntegrationEnvironment.Sandbox;
                    Options.PublicApiBaseUrl = "https://api.example.invalid";
                    Options.BackofficeReturnUrl = "https://backoffice.example.invalid/integrations";
                    Options.ContactEmail = "integrations@example.invalid";
                    Options.Sandbox.Enabled = true;
                    Options.Sandbox.ClientId = 123;
                    Options.Sandbox.ClientSecret = "test-only-client-secret";
                });
                services.Configure<ProviderIntegrationOptions>(Options => Options.KeyRingPath = "ephemeral-tests-only");
                services.AddSingleton<TimeProvider>(Clock);
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
