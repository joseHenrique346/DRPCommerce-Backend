using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateQueueEntryCommandHandler(IMediator mediator) : IRequestHandler<UpdateQueueEntryCommand, Result<QueueEntry>>
{
    public async Task<Result<QueueEntry>> Handle(UpdateQueueEntryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListQueueEntryCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<QueueEntry>.Success(result.Content.First())
            : Result<QueueEntry>.Failure(result.ListMessageErrors.First());
    }
}
