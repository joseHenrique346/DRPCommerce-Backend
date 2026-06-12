using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropAuditLogCommandHandler : IRequestHandler<UpdateListDropAuditLogCommand, Result<List<DropAuditLog>>>
{
    public Task<Result<List<DropAuditLog>>> Handle(UpdateListDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
