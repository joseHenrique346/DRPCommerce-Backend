using DropCommerce.Api.Controllers.Base;
using DropCommerce.Application.Features.Commands;
using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace DropCommerce.Api.Controllers;

[Route("api/drop-coupons")]
public class DropCouponController : BaseController<DropCoupon, CreateDropCouponCommand, CreateListDropCouponCommand, UpdateDropCouponCommand, UpdateListDropCouponCommand>
{
    public DropCouponController(IMediator mediator) : base(mediator)
    {
    }

    protected override CreateListDropCouponCommand WrapCreateInRange(CreateDropCouponCommand command)
    {
        return new CreateListDropCouponCommand(new List<CreateDropCouponCommand> { command });
    }

    protected override UpdateListDropCouponCommand WrapUpdateInRange(UpdateDropCouponCommand command)
    {
        return new UpdateListDropCouponCommand(new List<UpdateDropCouponCommand> { command });
    }

    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids)
    {
        return new DeleteListDropCouponCommand(ids);
    }

    protected override IRequest<Result<List<DropCoupon>>> GetAllQuery()
    {
        return new GetAllDropCouponQuery();
    }

    protected override IRequest<Result<DropCoupon>> GetByIdQuery(long id)
    {
        return new GetByIdDropCouponQuery(id);
    }

    protected override IRequest<Result<List<DropCoupon>>> GetListByListIdQuery(List<long> ids)
    {
        return new GetListByListIdDropCouponQuery(ids);
    }
}
