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

public sealed class AcknowledgeDropFulfillmentCommandHandler(
    IDropCheckoutRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    IStoreDropPayments payments)
    : IRequestHandler<AcknowledgeDropFulfillmentCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(AcknowledgeDropFulfillmentCommand command, CancellationToken ct)
    {
        long tenant = command.Tenant;
        long orderId = command.OrderId;

        persistence.ClearTracking();
        var order = await persistence.ReadOrderAsync(orderId, tenant, ct);

        if (!order.PaymentOperationId.HasValue || order.PaymentRevision == 0)
            return Result<bool>.Success(true);

        var closed = order.DropOrderStatusId is 5 or 6 or 7 && order.FinancialReviewCode is null;
        await payments.AcknowledgeAsync(tenant, order.PaymentOperationId.Value, order.PaymentRevision, closed, order.FinancialReviewCode is not null, ct);

        if (!closed)
            return Result<bool>.Success(true);

        using var operation = writes.Begin("sales", tenant);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        order = await persistence.GetOrderAsync(orderId, tenant, ct);
        order.AcknowledgeFulfillment();
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<bool>.Success(true);
    }
}
