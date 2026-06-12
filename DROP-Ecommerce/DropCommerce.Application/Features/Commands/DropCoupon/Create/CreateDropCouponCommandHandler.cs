using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropCouponCommandHandler(IMediator mediator) : IRequestHandler<CreateDropCouponCommand, Result<DropCoupon>>
{
    public async Task<Result<DropCoupon>> Handle(CreateDropCouponCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropCouponCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropCoupon>.Success(result.Content.First())
            : Result<DropCoupon>.Failure(result.ListMessageErrors.First());
    }
}
