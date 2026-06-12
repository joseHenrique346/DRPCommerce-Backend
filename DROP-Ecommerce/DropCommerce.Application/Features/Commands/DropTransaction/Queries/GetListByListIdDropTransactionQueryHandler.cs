using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropTransactionQueryHandler : IRequestHandler<GetListByListIdDropTransactionQuery, Result<List<DropTransaction>>>
{
    public Task<Result<List<DropTransaction>>> Handle(GetListByListIdDropTransactionQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
