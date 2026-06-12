using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateQueueEntryCommandHandler(IMediator mediator) : IRequestHandler<CreateQueueEntryCommand, Result<QueueEntry>>
{
    public async Task<Result<QueueEntry>> Handle(CreateQueueEntryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListQueueEntryCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<QueueEntry>.Success(result.Content.First())
            : Result<QueueEntry>.Failure(result.ListMessageErrors.First());
    }
}
