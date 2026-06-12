using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueSessionCommandHandler : IRequestHandler<DeleteListQueueSessionCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListQueueSessionCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
