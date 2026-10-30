namespace StoreCommerce.Domain.Models;

/// <summary>
/// Filtros da busca de clientes do backoffice. Term procura em nome, e-mail e telefone.
/// HasOrders considera compras concluídas (confirmadas até entregues). Sort: newest (padrão), oldest, name, total_spent_desc, last_order_desc.
/// </summary>
public sealed record CustomerSearchCriteria(
    string? Term, bool? IsActive, bool? IsVerified, bool? EmailConfirmed, long? StateId, bool? HasOrders,
    DateTime? CreatedFrom, DateTime? CreatedTo, string Sort, int Page, int PageSize);

public sealed record CustomerSearchRow(
    long Id, string FullName, string Email, string Phone, string City, long StateId, bool IsActive, bool IsVerified, bool EmailConfirmed,
    int CompletedOrders, decimal TotalSpent, DateTime? LastOrderAt, DateTime CreatedAt);
