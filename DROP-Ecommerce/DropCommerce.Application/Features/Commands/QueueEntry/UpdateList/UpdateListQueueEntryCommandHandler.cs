using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueEntryCommandHandler : IRequestHandler<UpdateListQueueEntryCommand, Result<List<QueueEntry>>>
{
    public Task<Result<List<QueueEntry>>> Handle(UpdateListQueueEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
