using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

public sealed record UpdateCartItemRequest(int Quantity);
