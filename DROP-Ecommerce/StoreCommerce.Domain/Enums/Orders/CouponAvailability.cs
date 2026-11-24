namespace StoreCommerce.Domain.Enums.Orders;

/// <summary>
/// Situação do cupom no momento da consulta.
/// </summary>
public enum CouponAvailability
{
    Scheduled = 1,
    Active = 2,
    Expired = 3,
    Exhausted = 4,
    Inactive = 5
}
