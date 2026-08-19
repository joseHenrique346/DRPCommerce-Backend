using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class UpdateListDropAuditLogCommandHandler(IRepository<DropAuditLog> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDropAuditLogCommand, UpdateListDropAuditLogCommand, DropAuditLog>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDropAuditLogCommand> GetCommandList(UpdateListDropAuditLogCommand request) => request.commands;

    protected override long GetById(UpdateDropAuditLogCommand command) => command.id;

    protected override void ApplyChanges(DropAuditLog entity, UpdateDropAuditLogCommand command)
    {
        entity.Update(command.dropEventId, command.customerId, command.employeeId, command.action, command.entityName, command.entityId, command.oldValues, command.newValues, command.ipAddress, command.userAgent, command.occurredAt);
    }
}
