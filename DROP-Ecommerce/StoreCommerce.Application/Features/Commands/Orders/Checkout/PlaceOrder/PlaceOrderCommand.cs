using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

// ---- Vitrine (cliente autenticado)

public sealed record PlaceOrderCommand(CheckoutRequest Request) : IRequest<Result<OrderDetailResponse>>;
