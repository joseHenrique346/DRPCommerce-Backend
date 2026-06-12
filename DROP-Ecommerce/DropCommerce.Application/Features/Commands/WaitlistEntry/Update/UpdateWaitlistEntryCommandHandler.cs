using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateWaitlistEntryCommandHandler(IMediator mediator) : IRequestHandler<UpdateWaitlistEntryCommand, Result<WaitlistEntry>>
{
    public async Task<Result<WaitlistEntry>> Handle(UpdateWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListWaitlistEntryCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<WaitlistEntry>.Success(result.Content.First())
            : Result<WaitlistEntry>.Failure(result.ListMessageErrors.First());
    }
}
