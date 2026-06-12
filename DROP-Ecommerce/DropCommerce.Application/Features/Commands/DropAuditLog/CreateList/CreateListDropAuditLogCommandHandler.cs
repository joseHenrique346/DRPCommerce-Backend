using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropAuditLogCommandHandler : IRequestHandler<CreateListDropAuditLogCommand, Result<List<DropAuditLog>>>
{
    public Task<Result<List<DropAuditLog>>> Handle(CreateListDropAuditLogCommand request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
