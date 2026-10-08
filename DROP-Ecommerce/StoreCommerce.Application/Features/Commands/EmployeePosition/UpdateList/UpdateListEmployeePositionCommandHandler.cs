using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEmployeePositionCommandHandler(IEmployeePositionRepository repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateEmployeePositionCommand, UpdateListEmployeePositionCommand, EmployeePosition>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateEmployeePositionCommand> GetCommandList(UpdateListEmployeePositionCommand request) => request.commands;

    protected override long GetById(UpdateEmployeePositionCommand command) => command.id;

    protected override void ApplyChanges(EmployeePosition entity, UpdateEmployeePositionCommand command) =>
        entity.UpdateDetails(command.name, command.description, command.isActive);
}
