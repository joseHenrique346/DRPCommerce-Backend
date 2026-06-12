using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdFraudSignalQueryHandler : IRequestHandler<GetListByListIdFraudSignalQuery, Result<List<FraudSignal>>>
{
    public Task<Result<List<FraudSignal>>> Handle(GetListByListIdFraudSignalQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
