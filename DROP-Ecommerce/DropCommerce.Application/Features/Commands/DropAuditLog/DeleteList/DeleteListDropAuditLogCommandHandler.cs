using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class DeleteListDropAuditLogCommandHandler(IRepository<DropAuditLog> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListDropAuditLogCommand, DropAuditLog>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListDropAuditLogCommand request) => request.ids;
}
