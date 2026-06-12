using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteQueueEntryCommandHandler(IMediator mediator) : IRequestHandler<DeleteQueueEntryCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteQueueEntryCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListQueueEntryCommand([request.id]), cancellationToken);
    }
}
