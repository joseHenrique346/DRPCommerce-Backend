namespace StoreCommerce.Application.Integrations;

public sealed class ProviderIntegrationOptions
{
    public string KeyRingPath { get; set; } = string.Empty;
    public string KeyEncryptionCertificateThumbprint { get; set; } = string.Empty;
}
