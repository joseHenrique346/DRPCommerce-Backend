using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

public sealed record AddCartItemRequest(long ProductId, long? ProductVariantId, int Quantity);
