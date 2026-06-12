using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListWaitlistEntryCommandHandler : IRequestHandler<UpdateListWaitlistEntryCommand, Result<List<WaitlistEntry>>>
{
    public Task<Result<List<WaitlistEntry>>> Handle(UpdateListWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
