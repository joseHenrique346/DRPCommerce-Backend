using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace StoreCommerce.Api.Controllers;

[Route("api/coupons")]
public class CouponController : BaseController<Coupon, CreateCouponCommand, CreateListCouponCommand, UpdateCouponCommand, UpdateListCouponCommand>
{
    public CouponController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListCouponCommand WrapCreateInRange(CreateCouponCommand command)
    {
        return new CreateListCouponCommand(new List<CreateCouponCommand> { command });
    }

    protected override UpdateListCouponCommand WrapUpdateInRange(UpdateCouponCommand command)
    {
        return new UpdateListCouponCommand(new List<UpdateCouponCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListCouponCommand(ids);
    }

    protected override IRequest<Result<List<Coupon>>> GetAllQuery()
    {
        return new GetAllCouponQuery();
    }

    protected override IRequest<Result<Coupon>> GetByIdQuery(long id)
    {
        return new GetByIdCouponQuery(id);
    }

    protected override IRequest<Result<List<Coupon>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdCouponQuery(ids);
    }
}
