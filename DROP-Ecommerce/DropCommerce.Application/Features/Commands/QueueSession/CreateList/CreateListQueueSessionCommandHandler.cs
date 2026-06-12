using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListQueueSessionCommandHandler : IRequestHandler<CreateListQueueSessionCommand, Result<List<QueueSession>>>
{
    public Task<Result<List<QueueSession>>> Handle(CreateListQueueSessionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
