using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Em processamento → Enviado, registrando o envio.
/// </summary>
public sealed record ShipOrderCommand(long Id, ShipOrderRequest Request) : IRequest<Result<OrderDetailResponse>>;
