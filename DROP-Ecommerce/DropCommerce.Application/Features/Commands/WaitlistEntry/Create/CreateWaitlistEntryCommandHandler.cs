using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateWaitlistEntryCommandHandler(IMediator mediator) : IRequestHandler<CreateWaitlistEntryCommand, Result<WaitlistEntry>>
{
    public async Task<Result<WaitlistEntry>> Handle(CreateWaitlistEntryCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListWaitlistEntryCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<WaitlistEntry>.Success(result.Content.First())
            : Result<WaitlistEntry>.Failure(result.ListMessageErrors.First());
    }
}
