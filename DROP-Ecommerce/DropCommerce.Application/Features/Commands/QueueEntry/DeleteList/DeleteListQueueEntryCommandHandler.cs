using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListQueueEntryCommandHandler : IRequestHandler<DeleteListQueueEntryCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListQueueEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
