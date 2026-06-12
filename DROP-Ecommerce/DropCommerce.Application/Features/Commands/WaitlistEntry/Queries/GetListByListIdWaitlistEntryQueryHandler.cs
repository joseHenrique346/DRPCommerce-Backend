using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdWaitlistEntryQueryHandler : IRequestHandler<GetListByListIdWaitlistEntryQuery, Result<List<WaitlistEntry>>>
{
    public Task<Result<List<WaitlistEntry>>> Handle(GetListByListIdWaitlistEntryQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
