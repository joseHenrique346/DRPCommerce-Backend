using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetDropBuyerProfileQueryHandler(
    IDropSalePaymentRepository persistence,
    IdentityHandlerContext context)
    : IdentityHandlerBase<GetDropBuyerProfileQuery, DropBuyerProfile>(context)
{
    protected override async Task<DropBuyerProfile> ExecuteAsync(GetDropBuyerProfileQuery command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long customerId = command.CustomerId;

        var customer = await persistence.FindCustomerAsync(customerId, tenant, ct)
            ?? throw new SecurityFailure(404, "DropPayment.CustomerUnavailable", "Comprador indisponível.");

        return new DropBuyerProfile(customer.Id, tenant, customer.FullName, customer.Email.Value,
            customer.AddressLine, customer.City, customer.State?.Description ?? "", customer.ZipCode);
    }
}
