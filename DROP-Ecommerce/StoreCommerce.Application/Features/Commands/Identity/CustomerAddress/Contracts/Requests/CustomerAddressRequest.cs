using StoreCommerce.Domain.StaticEntity;
using AddressEntity = StoreCommerce.Domain.Entity.CustomerAddress;

namespace StoreCommerce.Application.Contracts.Customers;

/// <summary>
/// Endereço de entrega. Country é opcional (padrão "Brasil"). IsDefault torna este o endereço padrão do checkout;
/// o primeiro endereço do cliente é sempre o padrão.
/// </summary>
public sealed record CustomerAddressRequest(string? Label, string? RecipientName, string AddressLine, string City, long StateId, string ZipCode, string? Country, bool IsDefault);
