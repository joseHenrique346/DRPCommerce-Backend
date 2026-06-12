using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropProductCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropProductCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropProductCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropProductCommand([request.id]), cancellationToken);
    }
}
