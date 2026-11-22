using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Order;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Busca de pedidos do backoffice. StatusIds e PaymentStatusIds aceitam vários valores; datas filtram a criação (UTC).
/// Sort: newest (padrão), oldest, total_desc, total_asc.
/// </summary>
public sealed record SearchOrdersQuery(
    long? Number = null, List<long>? StatusIds = null, List<long>? PaymentStatusIds = null, long? CustomerId = null, long? CouponId = null,
    DateTime? CreatedFrom = null, DateTime? CreatedTo = null, decimal? MinTotal = null, decimal? MaxTotal = null,
    string? Sort = null, int Page = 1, int PageSize = 20) : IRequest<Result<PagedResponse<OrderSearchItemResponse>>>;
