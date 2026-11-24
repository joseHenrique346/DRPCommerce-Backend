using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropCouponQueryHandler(IRepository<DropCoupon> repository)
    : BaseGetByIdHandler<GetByIdDropCouponQuery, DropCoupon>(repository)
{
    protected override long GetById(GetByIdDropCouponQuery request) => request.id;
}
