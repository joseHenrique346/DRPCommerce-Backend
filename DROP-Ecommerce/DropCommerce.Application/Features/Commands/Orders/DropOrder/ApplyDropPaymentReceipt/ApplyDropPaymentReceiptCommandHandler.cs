using DropCommerce.Application.Interfaces.Orders;
using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ApplyDropPaymentReceiptCommandHandler(
    IDropCheckoutRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    TimeProvider clock,
    IMediator mediator)
    : IRequestHandler<ApplyDropPaymentReceiptCommand, Result<bool>>
{
    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    public async Task<Result<bool>> Handle(ApplyDropPaymentReceiptCommand command, CancellationToken ct)
    {
        DropSalePaymentReceipt receipt = command.Receipt;

        persistence.ClearTracking();
        var eventId = await persistence.FindEventIdAsync(receipt.DropOrderId, receipt.EnterpriseId, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido DROP não encontrado.");
        await mediator.SendRequiredAsync(new ExpireDropPurchasesCommand(receipt.EnterpriseId, eventId), ct);
        persistence.ClearTracking();
        using var operation = writes.Begin("sales", receipt.EnterpriseId);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(receipt.EnterpriseId, ct);
        var order = await persistence.FindOrderForReceiptAsync(receipt.DropOrderId, receipt.EnterpriseId, ct)
            ?? throw new SecurityFailure(404, "DropPurchase.OrderNotFound", "Pedido DROP não encontrado.");

        if (order.PaymentOperationId != receipt.OperationId || order.CustomerId != receipt.CustomerId ||
            order.TotalAmount != receipt.Amount || receipt.Currency != "BRL" || receipt.Revision < 1 ||
            receipt.Status == "approved" && string.IsNullOrWhiteSpace(receipt.PaymentId))
            throw new SecurityFailure(409, "DropPurchase.PaymentMismatch", "A confirmação não corresponde ao pedido e ao comprador.");

        if (receipt.Revision < order.PaymentRevision)
            return Result<bool>.Success(true);

        if (receipt.Revision == order.PaymentRevision)
        {
            order.PaymentAccepted(receipt.CheckoutUrl);
            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);

            return Result<bool>.Success(true);
        }

        var reservation = order.DropReservation;
        var paid = receipt.Status == "approved";
        var late = paid && (Now >= reservation.ExpiresAt || reservation.DropReservationStatusId != 1);
        var alreadyPaid = order.DropOrderPaymentStatusId == 2;
        string? review = receipt.Status == "review_required" ? "DropPayment.FinancialReview" :
            late && !alreadyPaid ? "DropPayment.LateApproval" : null;

        if (paid && !late && !alreadyPaid)
        {
            foreach (var line in reservation.Lines)
                line.Variant.Commit(line.Quantity);

            reservation.Confirm(Now);
            var participation = await persistence.GetParticipationAsync(reservation.DropProductId, receipt.EnterpriseId, ct);
            participation.RegisterSale(reservation.Quantity);
        }

        order.PaymentAccepted(receipt.CheckoutUrl);
        order.ObservePayment(receipt.Revision, paid && !late && !alreadyPaid, review);
        order.RecordPaymentResolution(receipt.Status);
        order.RecordProviderPayment(receipt.PaymentId);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }
}
