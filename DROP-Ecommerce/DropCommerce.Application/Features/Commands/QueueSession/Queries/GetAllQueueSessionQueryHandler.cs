using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllQueueSessionQueryHandler : IRequestHandler<GetAllQueueSessionQuery, Result<List<QueueSession>>>
{
    public Task<Result<List<QueueSession>>> Handle(GetAllQueueSessionQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
