using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropEventCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropEventCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropEventCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropEventCommand([request.id]), cancellationToken);
    }
}
