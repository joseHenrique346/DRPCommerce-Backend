using StoreCommerce.Domain.StaticEntity;
using AddressEntity = StoreCommerce.Domain.Entity.CustomerAddress;

namespace StoreCommerce.Application.Contracts.Customers;

/// <summary>
/// Linha da busca de clientes. Compras e total gasto consideram pedidos concluídos (confirmados até entregues).
/// </summary>
public sealed record CustomerSearchItemResponse(long Id, string FullName, string Email, string Phone, string City, long? StateId, bool IsActive, bool IsVerified, bool EmailConfirmed, int CompletedOrders, decimal TotalSpent, DateTime? LastOrderAt, DateTime CreatedAt);
