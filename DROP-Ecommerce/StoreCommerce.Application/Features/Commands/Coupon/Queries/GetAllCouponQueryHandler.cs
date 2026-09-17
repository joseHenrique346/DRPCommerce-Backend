using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllCouponQueryHandler(IRepository<Coupon> repository)
    : BaseGetAllHandler<GetAllCouponQuery, Coupon>(repository);
