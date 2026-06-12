using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateFraudSignalCommandHandler(IMediator mediator) : IRequestHandler<CreateFraudSignalCommand, Result<FraudSignal>>
{
    public async Task<Result<FraudSignal>> Handle(CreateFraudSignalCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListFraudSignalCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<FraudSignal>.Success(result.Content.First())
            : Result<FraudSignal>.Failure(result.ListMessageErrors.First());
    }
}
