using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdCouponQueryHandler(IRepository<Coupon> repository)
    : BaseGetByIdHandler<GetByIdCouponQuery, Coupon>(repository)
{
    protected override long GetById(GetByIdCouponQuery request) => request.id;
}
