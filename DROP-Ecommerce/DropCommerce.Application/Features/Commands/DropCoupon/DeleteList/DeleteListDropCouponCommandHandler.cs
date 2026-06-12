using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropCouponCommandHandler : IRequestHandler<DeleteListDropCouponCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropCouponCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
