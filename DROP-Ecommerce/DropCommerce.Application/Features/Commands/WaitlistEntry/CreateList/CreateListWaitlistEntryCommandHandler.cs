using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListWaitlistEntryCommandHandler : IRequestHandler<CreateListWaitlistEntryCommand, Result<List<WaitlistEntry>>>
{
    public Task<Result<List<WaitlistEntry>>> Handle(CreateListWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
