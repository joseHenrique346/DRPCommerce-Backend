using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdDropAuditLogQueryHandler(IRepository<DropAuditLog> repository)
    : BaseGetByIdHandler<GetByIdDropAuditLogQuery, DropAuditLog>(repository)
{
    protected override long GetById(GetByIdDropAuditLogQuery request) => request.id;
}
