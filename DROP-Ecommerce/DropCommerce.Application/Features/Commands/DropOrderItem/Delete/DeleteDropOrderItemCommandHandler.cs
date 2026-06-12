using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropOrderItemCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropOrderItemCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropOrderItemCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropOrderItemCommand([request.id]), cancellationToken);
    }
}
