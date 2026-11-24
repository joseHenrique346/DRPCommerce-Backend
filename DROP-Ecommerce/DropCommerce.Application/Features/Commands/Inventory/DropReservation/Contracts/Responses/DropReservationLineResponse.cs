namespace DropCommerce.Application.Contracts;

public sealed record DropReservationLineResponse(
    long VariantId,
    string SKU,
    string ItemName,
    int Quantity,
    decimal UnitPrice,
    string OptionsJson);
