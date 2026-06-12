using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateFraudSignalCommandHandler(IMediator mediator) : IRequestHandler<UpdateFraudSignalCommand, Result<FraudSignal>>
{
    public async Task<Result<FraudSignal>> Handle(UpdateFraudSignalCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListFraudSignalCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<FraudSignal>.Success(result.Content.First())
            : Result<FraudSignal>.Failure(result.ListMessageErrors.First());
    }
}
