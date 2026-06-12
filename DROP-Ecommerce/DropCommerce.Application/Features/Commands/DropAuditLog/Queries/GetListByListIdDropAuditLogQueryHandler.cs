using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropAuditLogQueryHandler : IRequestHandler<GetListByListIdDropAuditLogQuery, Result<List<DropAuditLog>>>
{
    public Task<Result<List<DropAuditLog>>> Handle(GetListByListIdDropAuditLogQuery request, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}
