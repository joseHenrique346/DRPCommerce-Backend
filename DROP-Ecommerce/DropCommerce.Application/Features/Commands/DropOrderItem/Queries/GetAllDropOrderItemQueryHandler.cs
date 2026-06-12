using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropOrderItemQueryHandler : IRequestHandler<GetAllDropOrderItemQuery, Result<List<DropOrderItem>>>
{
    public Task<Result<List<DropOrderItem>>> Handle(GetAllDropOrderItemQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
