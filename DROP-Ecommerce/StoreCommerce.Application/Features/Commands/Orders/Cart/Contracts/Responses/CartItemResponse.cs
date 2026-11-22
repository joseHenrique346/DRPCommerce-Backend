using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

/// <summary>
/// Item do carrinho com estoque e preço atuais. RequiresDecision indica que o item ficou sem estoque ou indisponível
/// e o cliente precisa remover, ajustar a quantidade ou aguardar o estoque. BackInStock indica que um item em espera voltou.
/// </summary>
public sealed record CartItemResponse(
    long Id,
    long ProductId,
    long ProductVariantId,
    string ProductName,
    string ProductSlug,
    string VariantName,
    string VariantAttributesJson,
    string SKU,
    string ImageUrls,
    decimal UnitPrice,
    int Quantity,
    decimal LineTotal,
    int AvailableQuantity,
    bool TracksStock,
    CartItemAvailability Availability,
    bool IsSelected,
    bool IsAwaitingStock,
    bool RequiresDecision,
    bool BackInStock);
