using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RemoveCartItemsCommand(List<long> ItemIds) : IRequest<Result<CartResponse>>;
