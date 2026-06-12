using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteFraudSignalCommandHandler(IMediator mediator) : IRequestHandler<DeleteFraudSignalCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteFraudSignalCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListFraudSignalCommand([request.id]), cancellationToken);
    }
}
