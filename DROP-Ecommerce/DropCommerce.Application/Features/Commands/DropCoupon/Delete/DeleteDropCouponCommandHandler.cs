using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropCouponCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropCouponCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropCouponCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropCouponCommand([request.id]), cancellationToken);
    }
}
