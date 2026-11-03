using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Consulta o carrinho do cliente autenticado com estoque atual de cada item. CouponCode (opcional) simula o desconto
/// sobre os itens selecionados.
/// </summary>
public sealed record GetCartQuery(string? CouponCode) : IRequest<Result<CartResponse>>;

/// <summary>
/// Adiciona um produto ao carrinho. ProductVariantId é obrigatório em produtos com variantes.
/// </summary>
public sealed record AddCartItemCommand(long ProductId, long? ProductVariantId, int Quantity) : IRequest<Result<CartResponse>>;

public sealed record UpdateCartItemQuantityCommand(long ItemId, int Quantity) : IRequest<Result<CartResponse>>;

public sealed record RemoveCartItemsCommand(List<long> ItemIds) : IRequest<Result<CartResponse>>;

public sealed record ClearCartCommand : IRequest<Result<CartResponse>>;

/// <summary>
/// Marca ou desmarca itens para a compra. Sem ItemIds, aplica a todos ("selecionar todos" marca só os disponíveis).
/// </summary>
public sealed record SetCartSelectionCommand(List<long>? ItemIds, bool Selected) : IRequest<Result<CartResponse>>;

/// <summary>
/// Mantém um item sem estoque no carrinho, fora da compra, aguardando reposição.
/// </summary>
public sealed record AwaitCartItemStockCommand(long ItemId) : IRequest<Result<CartResponse>>;
