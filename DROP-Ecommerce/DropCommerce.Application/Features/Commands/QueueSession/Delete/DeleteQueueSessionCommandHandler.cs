using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteQueueSessionCommandHandler(IMediator mediator) : IRequestHandler<DeleteQueueSessionCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteQueueSessionCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListQueueSessionCommand([request.id]), cancellationToken);
    }
}
