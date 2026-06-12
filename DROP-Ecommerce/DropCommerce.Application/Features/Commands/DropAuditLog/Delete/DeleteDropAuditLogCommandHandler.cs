using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteDropAuditLogCommandHandler(IMediator mediator) : IRequestHandler<DeleteDropAuditLogCommand, Result<bool>>
{
    public async Task<Result<bool>> Handle(DeleteDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        return await mediator.Send(new DeleteListDropAuditLogCommand([request.id]), cancellationToken);
    }
}
