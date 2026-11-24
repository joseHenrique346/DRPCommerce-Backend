using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public static class CustomerAddressSelection
{
    public static string Country(CustomerAddressRequest request) =>
            string.IsNullOrWhiteSpace(request.Country) ? Shipment.DefaultCountry : request.Country;

    public static CustomerAddress Find(CustomerAddressBook book, long id) =>
            book.Addresses.FirstOrDefault(address => address.Id == id)
                ?? throw new SecurityFailure(404, "CustomerAddress.NotFound", "Endereço não encontrado.");
}
