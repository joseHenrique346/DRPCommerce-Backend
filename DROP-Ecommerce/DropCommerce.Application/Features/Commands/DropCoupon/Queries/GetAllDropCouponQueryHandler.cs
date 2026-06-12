using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropCouponQueryHandler : IRequestHandler<GetAllDropCouponQuery, Result<List<DropCoupon>>>
{
    public Task<Result<List<DropCoupon>>> Handle(GetAllDropCouponQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
