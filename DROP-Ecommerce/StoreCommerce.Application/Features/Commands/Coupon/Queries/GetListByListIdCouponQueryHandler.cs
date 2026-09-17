using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetListByListIdCouponQueryHandler(IRepository<Coupon> repository)
    : BaseGetListByListIdHandler<GetListByListIdCouponQuery, Coupon>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdCouponQuery request) => request.listId;
}
