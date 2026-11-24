using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Payments;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ListDueDropSalePaymentsQueryHandler(
    IDropSalePaymentRepository persistence,
    TimeProvider clock,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListDueDropSalePaymentsQuery, List<DropSalePaymentIntent>>(context)
{
    private new DateTime Now => clock.GetUtcNow().UtcDateTime;

    protected override async Task<List<DropSalePaymentIntent>> ExecuteAsync(ListDueDropSalePaymentsQuery command, CancellationToken ct)
    {
        return await persistence.ListDueAsync(Now, ct);
    }
}
