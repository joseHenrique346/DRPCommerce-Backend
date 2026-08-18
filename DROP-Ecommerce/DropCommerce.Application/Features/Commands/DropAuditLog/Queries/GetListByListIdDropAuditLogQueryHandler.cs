using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdDropAuditLogQueryHandler(IRepository<DropAuditLog> repository)
    : BaseGetListByListIdHandler<GetListByListIdDropAuditLogQuery, DropAuditLog>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdDropAuditLogQuery request) => request.listId;
}
