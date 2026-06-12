using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropProductQueryHandler : IRequestHandler<GetAllDropProductQuery, Result<List<DropProduct>>>
{
    public Task<Result<List<DropProduct>>> Handle(GetAllDropProductQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
