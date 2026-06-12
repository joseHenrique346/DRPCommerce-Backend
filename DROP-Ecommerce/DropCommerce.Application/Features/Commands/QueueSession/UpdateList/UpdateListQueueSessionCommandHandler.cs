using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListQueueSessionCommandHandler : IRequestHandler<UpdateListQueueSessionCommand, Result<List<QueueSession>>>
{
    public Task<Result<List<QueueSession>>> Handle(UpdateListQueueSessionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
