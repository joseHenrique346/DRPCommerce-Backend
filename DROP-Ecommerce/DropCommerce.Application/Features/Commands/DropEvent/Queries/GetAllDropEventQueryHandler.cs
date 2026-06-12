using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropEventQueryHandler : IRequestHandler<GetAllDropEventQuery, Result<List<DropEvent>>>
{
    public Task<Result<List<DropEvent>>> Handle(GetAllDropEventQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
