using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ClearCartCommand : IRequest<Result<CartResponse>>;
