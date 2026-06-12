using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropCouponCommandHandler : IRequestHandler<CreateListDropCouponCommand, Result<List<DropCoupon>>>
{
    public Task<Result<List<DropCoupon>>> Handle(CreateListDropCouponCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
