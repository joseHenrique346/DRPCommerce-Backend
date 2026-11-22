using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropCouponQueryHandler(IRepository<DropCoupon> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropCouponQuery, DropCoupon>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropCouponQuery request) => request.listId;
}
