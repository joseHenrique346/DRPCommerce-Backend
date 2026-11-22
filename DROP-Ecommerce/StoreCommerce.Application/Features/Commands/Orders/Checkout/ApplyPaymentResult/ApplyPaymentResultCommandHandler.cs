using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Orders;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ApplyPaymentResultCommandHandler(OrderLifecycleService lifecycle)    : WorkflowRequestHandler<ApplyPaymentResultCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(ApplyPaymentResultCommand request, CancellationToken ct)
    {
        var order = await lifecycle.ApplyPaymentResultAsync(request.Provider, request.Reference, request.Status, request.PaidAmount, ct);

        return OrderDetailResponse.FromEntity(order, backoffice: true);
    }
}
