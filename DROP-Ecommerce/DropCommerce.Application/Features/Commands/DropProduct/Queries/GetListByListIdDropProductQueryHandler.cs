using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropProductQueryHandler : IRequestHandler<GetListByListIdDropProductQuery, Result<List<DropProduct>>>
{
    public Task<Result<List<DropProduct>>> Handle(GetListByListIdDropProductQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
