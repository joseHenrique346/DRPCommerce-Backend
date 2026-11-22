using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Consulta o carrinho do cliente autenticado com estoque atual de cada item. CouponCode (opcional) simula o desconto
/// sobre os itens selecionados.
/// </summary>
public sealed record GetCartQuery(string? CouponCode) : IRequest<Result<CartResponse>>;
