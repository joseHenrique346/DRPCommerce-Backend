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

public sealed class ListPendingDropPaymentReceiptsQueryHandler(
    IDropSalePaymentRepository persistence,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ListPendingDropPaymentReceiptsQuery, IReadOnlyList<DropSalePaymentReceipt>>(context)
{
    protected override async Task<IReadOnlyList<DropSalePaymentReceipt>> ExecuteAsync(ListPendingDropPaymentReceiptsQuery command, CancellationToken ct)
    {
        var rows = await persistence.PendingReceiptsAsync(ct);

        return rows.Select(Receipt).ToList();
    }

    private static DropSalePaymentReceipt Receipt(DropSalePaymentIntent intent)
    {
        return new DropSalePaymentReceipt(intent.OperationId, intent.EnterpriseId, intent.DropOrderId, intent.CustomerId,
            intent.Amount, intent.Currency, intent.Status, intent.CheckoutUrl, intent.PaymentId, intent.Revision);
    }
}
