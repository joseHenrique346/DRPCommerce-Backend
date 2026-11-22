using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Enums.Orders;

/// <summary>
/// Quem originou a mudança de status do pedido.
/// </summary>
public enum OrderChangeSource
{
    Customer = 1,
    Backoffice = 2,
    System = 3,
    PaymentGateway = 4
}
