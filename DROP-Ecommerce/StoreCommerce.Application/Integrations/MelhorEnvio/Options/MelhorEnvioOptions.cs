using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Enums.Integrations;
using System.Net.Mail;

namespace StoreCommerce.Application.Integrations.MelhorEnvio;

public sealed class MelhorEnvioOptions
{
    public const string Route = "api/integrations/melhor-envio";
    public const string OAuthScopes = "users-read shipping-calculate";

    public string PublicApiBaseUrl { get; set; } = string.Empty;
    public string BackofficeReturnUrl { get; set; } = string.Empty;
    public string ContactEmail { get; set; } = string.Empty;
    public MelhorEnvioEnvironmentOptions Sandbox { get; set; } = new();
    public MelhorEnvioEnvironmentOptions Production { get; set; } = new();
    public IntegrationEnvironment? ShippingEnvironment { get; set; }

    public MelhorEnvioEnvironmentOptions RequireOAuth(IntegrationEnvironment environment)
    {
        var settings = environment switch
        {
            IntegrationEnvironment.Sandbox => Sandbox,
            IntegrationEnvironment.Production => Production,
            _ => throw new ArgumentOutOfRangeException(nameof(environment))
        };

        if (!settings.Enabled)
            throw new SecurityFailure(409, "MelhorEnvio.Disabled", "Melhor Envio não está habilitado neste ambiente.");

        if (settings.ClientId <= 0 || string.IsNullOrWhiteSpace(settings.ClientSecret) ||
            !MailAddress.TryCreate(ContactEmail, out var contact) || contact.Address != ContactEmail ||
            !IsPublicHttps(BackofficeReturnUrl, allowQuery: true) ||
            !IsPublicHttps(PublicApiBaseUrl, allowQuery: false))
            throw new SecurityFailure(409, "MelhorEnvio.ConfigurationRequired", "Configure as credenciais, o contato técnico e as URLs HTTPS do Melhor Envio.");

        return settings;
    }

    public string CallbackUrl(IntegrationEnvironment environment)
    {
        return $"{PublicApiBaseUrl.TrimEnd('/')}/{Route}/oauth/{environment.ToString().ToLowerInvariant()}/callback";
    }

    private static bool IsPublicHttps(string value, bool allowQuery)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri) && uri.Scheme == Uri.UriSchemeHttps &&
            !uri.IsLoopback && uri.UserInfo.Length == 0 && uri.Fragment.Length == 0 &&
            (allowQuery || uri.Query.Length == 0);
    }
}
