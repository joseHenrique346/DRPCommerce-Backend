using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Orders;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PlaceOrderCommandHandler(CheckoutService checkout) : WorkflowRequestHandler<PlaceOrderCommand, OrderDetailResponse>
{
    protected override Task<OrderDetailResponse> ExecuteAsync(PlaceOrderCommand request, CancellationToken ct) => checkout.PlaceOrderAsync(request.Request, ct);
}
