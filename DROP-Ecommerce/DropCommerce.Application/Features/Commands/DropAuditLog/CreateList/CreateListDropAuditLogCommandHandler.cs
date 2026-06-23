using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class CreateListDropAuditLogCommandHandler(IRepository<DropAuditLog> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateDropAuditLogCommand, CreateListDropAuditLogCommand, DropAuditLog>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateDropAuditLogCommand> GetCommandList(CreateListDropAuditLogCommand request) => request.commands;

    protected override DropAuditLog CreateEntity(CreateDropAuditLogCommand command) =>
        DropAuditLog.Create(command.dropEventId, command.customerId, command.employeeId, command.action, command.entityName, command.entityId, command.oldValues, command.newValues, command.ipAddress, command.userAgent, command.ocurredAt);
}
