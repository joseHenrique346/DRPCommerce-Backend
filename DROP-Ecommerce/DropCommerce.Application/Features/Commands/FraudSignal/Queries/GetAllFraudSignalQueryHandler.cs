using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllFraudSignalQueryHandler : IRequestHandler<GetAllFraudSignalQuery, Result<List<FraudSignal>>>
{
    public Task<Result<List<FraudSignal>>> Handle(GetAllFraudSignalQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
