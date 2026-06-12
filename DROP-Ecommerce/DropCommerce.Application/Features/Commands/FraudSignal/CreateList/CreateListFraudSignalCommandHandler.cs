using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListFraudSignalCommandHandler : IRequestHandler<CreateListFraudSignalCommand, Result<List<FraudSignal>>>
{
    public Task<Result<List<FraudSignal>>> Handle(CreateListFraudSignalCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
