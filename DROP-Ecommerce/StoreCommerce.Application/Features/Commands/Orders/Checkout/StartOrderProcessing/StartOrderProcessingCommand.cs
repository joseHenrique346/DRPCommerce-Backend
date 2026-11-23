using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Contracts.Order;

namespace StoreCommerce.Application.Features.Commands;

public sealed record StartOrderProcessingCommand(long Id) : IRequest<Result<OrderDetailResponse>>;
