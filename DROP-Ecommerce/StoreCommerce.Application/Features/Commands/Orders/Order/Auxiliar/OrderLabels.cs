using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

internal static class OrderLabels
{
    public static string Status(long id) => BaseStaticEntity.GetAll<OrderStatus>().First(status => status.Id == id).Description;

    public static string PaymentStatus(long id) => BaseStaticEntity.GetAll<OrderPaymentStatus>().First(status => status.Id == id).Description;
}
