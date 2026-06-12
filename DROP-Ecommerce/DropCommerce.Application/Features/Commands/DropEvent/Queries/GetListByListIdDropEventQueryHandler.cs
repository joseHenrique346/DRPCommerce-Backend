using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropEventQueryHandler : IRequestHandler<GetListByListIdDropEventQuery, Result<List<DropEvent>>>
{
    public Task<Result<List<DropEvent>>> Handle(GetListByListIdDropEventQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
