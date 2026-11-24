namespace StoreCommerce.Domain.Entity;

public sealed record ShippingVolume(decimal WeightKg, decimal HeightCm, decimal WidthCm, decimal LengthCm, decimal InsuranceValue);
