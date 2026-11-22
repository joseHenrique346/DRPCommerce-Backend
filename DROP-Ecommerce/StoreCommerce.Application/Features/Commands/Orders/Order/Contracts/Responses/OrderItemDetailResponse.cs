using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record OrderItemDetailResponse(
    long Id, long? ProductId, long? ProductVariantId, long? ServiceId, string ItemName, string VariantName, string SKU,
    int Quantity, decimal UnitPrice, decimal DiscountAmount, decimal TotalPrice);
