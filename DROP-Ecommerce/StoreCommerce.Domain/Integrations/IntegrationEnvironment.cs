namespace StoreCommerce.Domain.Integrations;

public enum IntegrationEnvironment
{
    Sandbox = 1,
    Production = 2
}

public static class IntegrationProviders
{
    public const string Manual = "manual";
    public const string MercadoPago = "mercado-pago";
    public const string MelhorEnvio = "melhor-envio";

    public static void Validate(string provider, IntegrationEnvironment environment)
    {
        if (provider is not (Manual or MercadoPago or MelhorEnvio) || !Enum.IsDefined(environment))
            throw new ArgumentException("Provedor ou ambiente de integração inválido.");
    }
}
