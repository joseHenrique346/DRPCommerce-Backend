namespace StoreCommerce.Domain.Enums.Orders;

/// <summary>
/// Situação do item no momento da consulta.
/// </summary>
public enum CartItemAvailability
{
    /// <summary>Pode ser comprado na quantidade do carrinho.</summary>
    Available = 1,
    /// <summary>Há estoque, mas menos que a quantidade do carrinho.</summary>
    InsufficientStock = 2,
    /// <summary>Sem estoque.</summary>
    OutOfStock = 3,
    /// <summary>Produto ou SKU inativo, excluído ou fora da vitrine.</summary>
    Unavailable = 4
}
