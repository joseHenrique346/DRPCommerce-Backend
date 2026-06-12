using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropAuditLogQueryHandler : IRequestHandler<GetAllDropAuditLogQuery, Result<List<DropAuditLog>>>
{
    public Task<Result<List<DropAuditLog>>> Handle(GetAllDropAuditLogQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
