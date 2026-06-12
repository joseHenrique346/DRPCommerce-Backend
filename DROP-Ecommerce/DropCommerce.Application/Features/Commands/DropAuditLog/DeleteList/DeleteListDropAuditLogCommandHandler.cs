using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropAuditLogCommandHandler : IRequestHandler<DeleteListDropAuditLogCommand, Result<bool>>
{
    public Task<Result<bool>> Handle(DeleteListDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
