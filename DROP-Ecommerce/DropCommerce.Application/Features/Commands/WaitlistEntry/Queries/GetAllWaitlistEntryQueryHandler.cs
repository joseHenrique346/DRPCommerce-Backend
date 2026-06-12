using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllWaitlistEntryQueryHandler : IRequestHandler<GetAllWaitlistEntryQuery, Result<List<WaitlistEntry>>>
{
    public Task<Result<List<WaitlistEntry>>> Handle(GetAllWaitlistEntryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
