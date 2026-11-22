using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Adiciona um produto ao carrinho. ProductVariantId é obrigatório em produtos com variantes.
/// </summary>
public sealed record AddCartItemCommand(long ProductId, long? ProductVariantId, int Quantity) : IRequest<Result<CartResponse>>;
