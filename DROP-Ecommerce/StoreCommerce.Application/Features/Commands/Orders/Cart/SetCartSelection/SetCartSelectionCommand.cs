using MediatR;
using StoreCommerce.Application.Contracts.Cart;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Marca ou desmarca itens para a compra. Sem ItemIds, aplica a todos ("selecionar todos" marca só os disponíveis).
/// </summary>
public sealed record SetCartSelectionCommand(List<long>? ItemIds, bool Selected) : IRequest<Result<CartResponse>>;
