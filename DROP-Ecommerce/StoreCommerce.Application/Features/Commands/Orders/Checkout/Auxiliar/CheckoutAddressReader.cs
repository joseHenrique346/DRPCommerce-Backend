using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CheckoutAddressReader(ICustomerAddressRepository addresses, IRepository<Enterprise> enterprises)
{
    public async Task<CheckoutDeliveryAddress> ResolveAsync(ShippingAddressRequest request, Customer customer, CancellationToken ct)
    {
        if (!request.AddressId.HasValue && !request.UseProfileAddress)
            return new CheckoutDeliveryAddress(request.ShippingAddressLine!, request.ShippingCity!, request.ShippingStateId!.Value, request.ShippingZipCode!);

        var saved = await addresses.ListForCustomerAsync(customer.EnterpriseId, customer.Id, tracking: false, ct);
        var address = request.AddressId.HasValue
            ? saved.FirstOrDefault(Address => Address.Id == request.AddressId)
                ?? throw new SecurityFailure(404, "CustomerAddress.NotFound", "Endereço não encontrado.")
            : saved.FirstOrDefault(Address => Address.IsDefault);

        if (address is not null)
            return new CheckoutDeliveryAddress(address.AddressLine, address.City, address.StateId, address.ZipCode);

        if (string.IsNullOrWhiteSpace(customer.AddressLine) || string.IsNullOrWhiteSpace(customer.City) ||
            string.IsNullOrWhiteSpace(customer.ZipCode) || !customer.StateId.HasValue)
            throw new SecurityFailure(400, "Checkout.AddressRequired", "Cadastre um endereço de entrega para finalizar a compra.");

        return new CheckoutDeliveryAddress(customer.AddressLine, customer.City, customer.StateId.Value, customer.ZipCode);
    }

    public async Task<CheckoutDeliveryAddress> StoreAsync(long enterpriseId, CancellationToken ct)
    {
        return await enterprises.FindValueAsync(Enterprise => Enterprise.Id == enterpriseId && Enterprise.IsActive,
            Enterprise => new CheckoutDeliveryAddress(Enterprise.AddressLine, Enterprise.City, Enterprise.StateId, Enterprise.ZipCode),
            ct) ?? throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");
    }
}
