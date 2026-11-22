using StoreCommerce.Domain.Enums.Orders;
using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

public sealed record BuyerSnapshotResponse(string Name, string Email, string Phone, DateTime? CapturedAt, BuyerSnapshotSource? Source);
