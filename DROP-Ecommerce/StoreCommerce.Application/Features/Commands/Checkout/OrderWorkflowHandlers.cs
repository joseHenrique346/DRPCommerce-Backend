using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Converte as recusas esperadas dos fluxos de pedido (regras de domínio e falhas de negócio) em Result com a mensagem.
/// </summary>
public abstract class WorkflowRequestHandler<TRequest, TContent> : IRequestHandler<TRequest, Result<TContent>>
    where TRequest : IRequest<Result<TContent>>
{
    public async Task<Result<TContent>> Handle(TRequest request, CancellationToken cancellationToken)
    {
        try
        {
            return Result<TContent>.Success(await ExecuteAsync(request, cancellationToken));
        }
        catch (SecurityFailure failure)
        {
            return Result<TContent>.Failure(IdentityError.FromFailure(failure));
        }
        catch (ArgumentException exception)
        {
            return Result<TContent>.Validation("Order.InvalidOperation", exception.Message);
        }
    }

    protected abstract Task<TContent> ExecuteAsync(TRequest request, CancellationToken ct);
}

public sealed class PlaceOrderCommandHandler(CheckoutService checkout) : WorkflowRequestHandler<PlaceOrderCommand, OrderDetailResponse>
{
    protected override Task<OrderDetailResponse> ExecuteAsync(PlaceOrderCommand request, CancellationToken ct) => checkout.PlaceOrderAsync(request.Request, ct);
}

public sealed class GetMyOrdersQueryHandler(IOrderRepository orders, ICurrentIdentity actor)
    : WorkflowRequestHandler<GetMyOrdersQuery, PagedResponse<OrderSummaryResponse>>
{
    protected override async Task<PagedResponse<OrderSummaryResponse>> ExecuteAsync(GetMyOrdersQuery request, CancellationToken ct)
    {
        var (items, count) = await orders.GetPageForCustomerAsync(actor.CustomerId, request.Page, request.PageSize, ct);

        return new(items.Select(OrderSummaryResponse.FromEntity).ToList(), count, request.Page, request.PageSize);
    }
}

public sealed class GetMyOrderQueryHandler(IOrderRepository orders, ICurrentIdentity actor) : WorkflowRequestHandler<GetMyOrderQuery, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(GetMyOrderQuery request, CancellationToken ct) =>
        OrderDetailResponse.FromEntity(await orders.GetDetailAsync(request.Id, actor.CustomerId, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado."), backoffice: false);
}

public sealed class CancelMyOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)
    : WorkflowRequestHandler<CancelMyOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(CancelMyOrderCommand request, CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var reason = string.IsNullOrWhiteSpace(request.Reason) ? "Cancelado pelo cliente" : request.Reason.Trim();
        await lifecycle.CancelAsync(tenant.GetEnterpriseId(), request.Id, reason, OrderChangeSource.Customer, customerId, customerId, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, customerId, ct))!, backoffice: false);
    }
}

public sealed class GetOrderDetailQueryHandler(IOrderRepository orders) : WorkflowRequestHandler<GetOrderDetailQuery, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(GetOrderDetailQuery request, CancellationToken ct) =>
        OrderDetailResponse.FromEntity(await orders.GetDetailAsync(request.Id, null, ct)
            ?? throw new SecurityFailure(404, "Order.NotFound", "Pedido não encontrado."), backoffice: true);
}

public sealed class ConfirmOrderPaymentCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)
    : WorkflowRequestHandler<ConfirmOrderPaymentCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(ConfirmOrderPaymentCommand request, CancellationToken ct)
    {
        await lifecycle.ConfirmPaymentAsync(tenant.GetEnterpriseId(), request.Id, OrderChangeSource.Backoffice, actor.UserId,
            request.TransactionMethodId, request.GatewayReference, null, request.Note ?? "Pagamento confirmado manualmente", ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}

public sealed class AdvanceOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)
    : WorkflowRequestHandler<AdvanceOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(AdvanceOrderCommand request, CancellationToken ct)
    {
        await lifecycle.AdvanceAsync(tenant.GetEnterpriseId(), request.Id, request.Step, OrderChangeSource.Backoffice, actor.UserId, request.Note, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}

public sealed class CancelOrderCommandHandler(OrderLifecycleService lifecycle, IOrderRepository orders, ICurrentIdentity actor, ITenantProvider tenant)
    : WorkflowRequestHandler<CancelOrderCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(CancelOrderCommand request, CancellationToken ct)
    {
        await lifecycle.CancelAsync(tenant.GetEnterpriseId(), request.Id, request.Reason.Trim(), OrderChangeSource.Backoffice, actor.UserId, null, ct);

        return OrderDetailResponse.FromEntity((await orders.GetDetailAsync(request.Id, null, ct))!, backoffice: true);
    }
}

public sealed class ApplyPaymentResultCommandHandler(OrderLifecycleService lifecycle)
    : WorkflowRequestHandler<ApplyPaymentResultCommand, OrderDetailResponse>
{
    protected override async Task<OrderDetailResponse> ExecuteAsync(ApplyPaymentResultCommand request, CancellationToken ct)
    {
        var order = await lifecycle.ApplyPaymentResultAsync(request.Provider, request.Reference, request.Status, request.PaidAmount, ct);

        return OrderDetailResponse.FromEntity(order, backoffice: true);
    }
}
