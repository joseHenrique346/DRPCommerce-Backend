using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListFraudSignalCommandHandler : IRequestHandler<DeleteListFraudSignalCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListFraudSignalCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
