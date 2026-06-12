using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropOrderItemQueryHandler : IRequestHandler<GetListByListIdDropOrderItemQuery, Result<List<DropOrderItem>>>
{
    public Task<Result<List<DropOrderItem>>> Handle(GetListByListIdDropOrderItemQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
