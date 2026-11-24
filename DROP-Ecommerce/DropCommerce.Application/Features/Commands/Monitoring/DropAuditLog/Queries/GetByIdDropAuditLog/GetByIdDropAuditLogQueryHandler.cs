using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropAuditLogQueryHandler(IRepository<DropAuditLog> repository)
    : BaseGetByIdHandler<GetByIdDropAuditLogQuery, DropAuditLog>(repository)
{
    protected override long GetById(GetByIdDropAuditLogQuery request) => request.id;
}
