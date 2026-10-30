using StoreCommerce.Domain.StaticEntity;
using AddressEntity = StoreCommerce.Domain.Entity.CustomerAddress;

namespace StoreCommerce.Application.Contracts.Customers;

/// <summary>
/// Endereço de entrega. Country é opcional (padrão "Brasil"). IsDefault torna este o endereço padrão do checkout;
/// o primeiro endereço do cliente é sempre o padrão.
/// </summary>
public sealed record CustomerAddressRequest(string? Label, string? RecipientName, string AddressLine, string City, long StateId,
    string ZipCode, string? Country, bool IsDefault);

public sealed record CustomerAddressResponse(long Id, string Label, string RecipientName, string AddressLine, string City, long StateId,
    string State, string ZipCode, string Country, bool IsDefault)
{
    public static CustomerAddressResponse FromEntity(AddressEntity address) => new(
        address.Id, address.Label, address.RecipientName, address.AddressLine, address.City, address.StateId,
        BaseStaticEntity.GetAll<State>().First(state => state.Id == address.StateId).Description,
        address.ZipCode, address.Country, address.IsDefault);
}

/// <summary>
/// Linha da busca de clientes. Compras e total gasto consideram pedidos concluídos (confirmados até entregues).
/// </summary>
public sealed record CustomerSearchItemResponse(
    long Id, string FullName, string Email, string Phone, string City, long StateId, bool IsActive, bool IsVerified, bool EmailConfirmed,
    int CompletedOrders, decimal TotalSpent, DateTime? LastOrderAt, DateTime CreatedAt);
