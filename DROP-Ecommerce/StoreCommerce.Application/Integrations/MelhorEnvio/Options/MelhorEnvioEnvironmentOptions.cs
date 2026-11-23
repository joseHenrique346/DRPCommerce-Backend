namespace StoreCommerce.Application.Integrations.MelhorEnvio;

public sealed class MelhorEnvioEnvironmentOptions
{
    public bool Enabled { get; set; }
    public int ClientId { get; set; }
    public string ClientSecret { get; set; } = string.Empty;
}
