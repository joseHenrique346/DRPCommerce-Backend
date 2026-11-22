using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Cart;

/// <summary>
/// ItemIds nulo ou vazio aplica a todos os itens ("selecionar todos" marca apenas os disponíveis).
/// </summary>
public sealed record CartSelectionRequest(List<long>? ItemIds, bool Selected);
