namespace StoreCommerce.Application.Integrations.MelhorEnvio;

public sealed record MelhorEnvioQuoteProduct(long VariantId, decimal WeightKg, decimal HeightCm,
    decimal WidthCm, decimal LengthCm, decimal UnitValue, int Quantity);
