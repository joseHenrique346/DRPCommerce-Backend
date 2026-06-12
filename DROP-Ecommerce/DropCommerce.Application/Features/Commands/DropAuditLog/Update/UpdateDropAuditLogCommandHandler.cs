using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateDropAuditLogCommandHandler(IMediator mediator) : IRequestHandler<UpdateDropAuditLogCommand, Result<DropAuditLog>>
{
    public async Task<Result<DropAuditLog>> Handle(UpdateDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new UpdateListDropAuditLogCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropAuditLog>.Success(result.Content.First())
            : Result<DropAuditLog>.Failure(result.ListMessageErrors.First());
    }
}
