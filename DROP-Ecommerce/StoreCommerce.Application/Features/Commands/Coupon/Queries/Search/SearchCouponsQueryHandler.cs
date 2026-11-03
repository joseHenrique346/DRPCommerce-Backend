using MediatR;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Application.Contracts.Coupon;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SearchCouponsQueryHandler(ICouponRepository repository, TimeProvider time)
    : IRequestHandler<SearchCouponsQuery, Result<PagedResponse<CouponResponse>>>
{
    public async Task<Result<PagedResponse<CouponResponse>>> Handle(SearchCouponsQuery request, CancellationToken cancellationToken)
    {
        var now = time.GetUtcNow().UtcDateTime;
        var (items, count) = await repository.SearchAsync(request.Code, request.TypeId, request.IsActive, request.Availability, now,
            request.Page, request.PageSize, cancellationToken);

        return Result<PagedResponse<CouponResponse>>.Success(new(
            items.Select(coupon => CouponResponse.FromEntity(coupon, now)).ToList(), count, request.Page, request.PageSize));
    }
}
