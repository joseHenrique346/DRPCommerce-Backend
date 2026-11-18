using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoOptions
{
    public bool Enabled { get; set; }
    public bool AllowProduction { get; set; }
    public string ClientId { get; set; } = string.Empty;
    public string ClientSecret { get; set; } = string.Empty;
    public string PublicApiBaseUrl { get; set; } = string.Empty;
    public string BackofficeReturnUrl { get; set; } = string.Empty;
    public string PaymentReturnUrl { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string SandboxWebhookSecret { get; set; } = string.Empty;
    public string SandboxAccessToken { get; set; } = string.Empty;
    public string StatementDescriptor { get; set; } = "DRPCOMMERCE";
    public int SignatureToleranceSeconds { get; set; } = 600;

    public string OAuthCallbackUrl => PublicApiBaseUrl.TrimEnd('/') + "/api/integrations/mercado-pago/oauth/callback";

    public string WebhookUrl(long connectionId)
    {
        return PublicApiBaseUrl.TrimEnd('/') + $"/api/integrations/mercado-pago/webhooks/{connectionId}";
    }

    public string SignatureSecret(IntegrationEnvironment environment)
    {
        return environment == IntegrationEnvironment.Sandbox ? SandboxWebhookSecret : WebhookSecret;
    }

    public void RequireEnvironment(IntegrationEnvironment environment)
    {
        IntegrationProviders.Validate(IntegrationProviders.MercadoPago, environment);

        if (!Enabled || environment == IntegrationEnvironment.Production && !AllowProduction)
            throw new SecurityFailure(409, "MercadoPago.Disabled", "Mercado Pago não está habilitado neste ambiente.");
    }

    public void RequireCheckout(IntegrationEnvironment environment)
    {
        RequireEnvironment(environment);
        RequireApiBaseUrl();
        RequireHttps(PaymentReturnUrl);

        if (string.IsNullOrWhiteSpace(SignatureSecret(environment)) || StatementDescriptor.Length is < 1 or > 22)
            throw new SecurityFailure(409, "MercadoPago.ConfigurationRequired", "Configure a assinatura do webhook e o descritor da integração.");
    }

    public void RequireOAuth(IntegrationEnvironment environment)
    {
        RequireEnvironment(environment);
        RequireApiBaseUrl();
        RequireHttps(BackofficeReturnUrl);

        if (string.IsNullOrWhiteSpace(ClientId) || string.IsNullOrWhiteSpace(ClientSecret))
            throw new SecurityFailure(409, "MercadoPago.ConfigurationRequired", "Configure as credenciais OAuth da aplicação.");
    }

    private void RequireApiBaseUrl()
    {
        RequireHttps(PublicApiBaseUrl);

        if (!string.IsNullOrEmpty(new Uri(PublicApiBaseUrl).Query))
            throw new SecurityFailure(409, "MercadoPago.PublicUrlRequired", "A base pública da API não pode conter query string.");
    }

    private static void RequireHttps(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment) || uri.IsLoopback)
            throw new SecurityFailure(409, "MercadoPago.PublicUrlRequired", "Configure uma URL HTTPS pública para a integração.");
    }
}
