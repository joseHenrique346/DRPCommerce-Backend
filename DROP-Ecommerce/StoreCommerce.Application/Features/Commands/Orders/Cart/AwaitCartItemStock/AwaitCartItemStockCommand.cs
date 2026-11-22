using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Mantém um item sem estoque no carrinho, fora da compra, aguardando reposição.
/// </summary>
public sealed record AwaitCartItemStockCommand(long ItemId) : IRequest<Result<CartResponse>>;
