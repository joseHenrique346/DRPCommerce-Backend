namespace StoreCommerce.Application.Contracts.Store;

/// <summary>
/// Dados públicos da loja exibidos na vitrine (sem informações internas).
/// </summary>
public sealed record PublicStoreInfoResponse(
    string TradeName,
    string Email,
    string Phone,
    string CurrencyCode,
    decimal MinOrderValue,
    int? MaxItemsPerOrder);
