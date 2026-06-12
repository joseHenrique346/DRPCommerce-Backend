using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropCouponQueryHandler : IRequestHandler<GetListByListIdDropCouponQuery, Result<List<DropCoupon>>>
{
    public Task<Result<List<DropCoupon>>> Handle(GetListByListIdDropCouponQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
