using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Contracts.Order;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeliverOrderCommand(long Id) : IRequest<Result<OrderDetailResponse>>;
