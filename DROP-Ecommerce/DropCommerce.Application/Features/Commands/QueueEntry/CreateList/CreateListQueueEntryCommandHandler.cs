using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueEntryCommandHandler : IRequestHandler<CreateListQueueEntryCommand, Result<List<QueueEntry>>>
{
    public Task<Result<List<QueueEntry>>> Handle(CreateListQueueEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
