using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropOrderQueryHandler : IRequestHandler<GetAllDropOrderQuery, Result<List<DropOrder>>>
{
    public Task<Result<List<DropOrder>>> Handle(GetAllDropOrderQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
