namespace StoreCommerce.Domain.Models;

/// <summary>
/// Filtros da busca de pedidos do backoffice. Datas filtram a criação do pedido (UTC); valores filtram o total.
/// Sort: newest (padrão), oldest, total_desc, total_asc.
/// </summary>
public sealed record OrderSearchCriteria(
    long? OrderNumber, IReadOnlyCollection<long>? StatusIds, IReadOnlyCollection<long>? PaymentStatusIds, long? CustomerId,
    long? CouponId, DateTime? CreatedFrom, DateTime? CreatedTo, decimal? MinTotal, decimal? MaxTotal,
    string Sort, int Page, int PageSize);
