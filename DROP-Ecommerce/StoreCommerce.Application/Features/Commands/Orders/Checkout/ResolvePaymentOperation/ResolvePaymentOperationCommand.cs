using MediatR;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ResolvePaymentOperationCommand(long OrderId, long OperationId, string? Reference, string? Note, bool Retry) : IRequest<Result<OrderDetailResponse>>;
