using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropCouponCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropCouponCommand, Result<DropCoupon>>
{
    public async Task<Result<DropCoupon>> Handle(UpdateDropCouponCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropCouponCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropCoupon>.Success(result.Content.First())
            : Result<DropCoupon>.Failure(result.ListMessageErrors.First());
    }
}
