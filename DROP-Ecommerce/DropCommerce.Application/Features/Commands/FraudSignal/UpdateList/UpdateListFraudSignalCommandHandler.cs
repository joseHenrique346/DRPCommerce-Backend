using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListFraudSignalCommandHandler : IRequestHandler<UpdateListFraudSignalCommand, Result<List<FraudSignal>>>
{
    public Task<Result<List<FraudSignal>>> Handle(UpdateListFraudSignalCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
