using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Billing;

/// <summary>
/// Conta recebedora da plataforma, independente do OAuth e das credenciais dos lojistas.
/// Segredos devem vir de configuração privada, nunca do catálogo ou do navegador.
/// </summary>
public sealed class PlatformBillingOptions
{
    public bool Enabled { get; set; }
    public bool AllowProduction { get; set; }
    public IntegrationEnvironment Environment { get; set; } = IntegrationEnvironment.Sandbox;
    public string AccountId { get; set; } = string.Empty;
    public string AccessToken { get; set; } = string.Empty;
    public string WebhookSecret { get; set; } = string.Empty;
    public string PublicApiBaseUrl { get; set; } = string.Empty;
    public string ReturnUrl { get; set; } = string.Empty;
    public int SignatureToleranceSeconds { get; set; } = 600;

    public string WebhookUrl => PublicApiBaseUrl.TrimEnd('/') + "/api/platform-billing/mercado-pago/webhook";

    public void RequireAccount()
    {
        if (!Enabled || !Enum.IsDefined(Environment) ||
            Environment == IntegrationEnvironment.Production && !AllowProduction)
            throw new SecurityFailure(409, "Billing.Disabled", "A cobrança da plataforma não está habilitada neste ambiente.");

        if (AccountId.Length is < 1 or > 40 || AccountId.Any(Character => !char.IsAsciiDigit(Character)) ||
            string.IsNullOrWhiteSpace(AccessToken))
            throw new SecurityFailure(409, "Billing.ConfigurationRequired", "Configure a conta recebedora e a credencial da plataforma.");
    }

    public void RequireCheckout()
    {
        RequireAccount();
        RequireHttps(PublicApiBaseUrl);
        RequireHttps(ReturnUrl);

        if (!string.IsNullOrEmpty(new Uri(PublicApiBaseUrl).Query) || string.IsNullOrWhiteSpace(WebhookSecret))
            throw new SecurityFailure(409, "Billing.ConfigurationRequired", "Configure a URL pública e a assinatura da cobrança da plataforma.");
    }

    private static void RequireHttps(string value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            uri.IsLoopback || !string.IsNullOrEmpty(uri.UserInfo) || !string.IsNullOrEmpty(uri.Fragment))
            throw new SecurityFailure(409, "Billing.PublicUrlRequired", "Configure uma URL HTTPS pública para a cobrança da plataforma.");
    }
}
