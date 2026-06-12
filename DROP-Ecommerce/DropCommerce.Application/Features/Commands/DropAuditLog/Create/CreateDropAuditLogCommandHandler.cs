using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateDropAuditLogCommandHandler(IMediator mediator) : IRequestHandler<CreateDropAuditLogCommand, Result<DropAuditLog>>
{
    public async Task<Result<DropAuditLog>> Handle(CreateDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        var result = await mediator.Send(new CreateListDropAuditLogCommand([request]), cancellationToken);
        return result.IsSuccess
            ? Result<DropAuditLog>.Success(result.Content.First())
            : Result<DropAuditLog>.Failure(result.ListMessageErrors.First());
    }
}
