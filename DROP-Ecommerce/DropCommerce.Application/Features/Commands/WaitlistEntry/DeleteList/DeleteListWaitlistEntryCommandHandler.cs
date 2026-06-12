using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListWaitlistEntryCommandHandler : IRequestHandler<DeleteListWaitlistEntryCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
