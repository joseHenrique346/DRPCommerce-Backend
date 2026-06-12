using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropCouponQueryHandler(IMediator mediator) : IRequestHandler<GetByIdDropCouponQuery, Result<DropCoupon>>
{
    public async Task<Result<DropCoupon>> Handle(GetByIdDropCouponQuery request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new GetListByListIdDropCouponQuery([request.id]), cancellationToken);
        return result.IsSuccess && result.Content.Count > 0
            ? Result<DropCoupon>.Success(result.Content.First())
            : Result<DropCoupon>.Failure("DropCoupon não encontrado.");
    }
}
