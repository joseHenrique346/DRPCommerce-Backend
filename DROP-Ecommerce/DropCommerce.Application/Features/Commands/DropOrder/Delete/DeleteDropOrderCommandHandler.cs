using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropOrderCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropOrderCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropOrderCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropOrderCommand([request.id]), cancellationToken);
    }
}
