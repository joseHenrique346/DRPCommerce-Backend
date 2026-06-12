using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllQueueEntryQueryHandler : IRequestHandler<GetAllQueueEntryQuery, Result<List<QueueEntry>>>
{
    public Task<Result<List<QueueEntry>>> Handle(GetAllQueueEntryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
