using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropTransactionQueryHandler : IRequestHandler<GetAllDropTransactionQuery, Result<List<DropTransaction>>>
{
    public Task<Result<List<DropTransaction>>> Handle(GetAllDropTransactionQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
