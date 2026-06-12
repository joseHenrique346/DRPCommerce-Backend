using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropCouponCommandHandler : IRequestHandler<UpdateListDropCouponCommand, Result<List<DropCoupon>>>
{
    public Task<Result<List<DropCoupon>>> Handle(UpdateListDropCouponCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
