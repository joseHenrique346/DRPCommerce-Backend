using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEmployeePositionCommandHandler(IEmployeePositionRepository repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateEmployeePositionCommand, CreateListEmployeePositionCommand, EmployeePosition>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEmployeePositionCommand> GetCommandList(CreateListEmployeePositionCommand request) => request.commands;

    protected override EmployeePosition CreateEntity(CreateEmployeePositionCommand command) =>
        EmployeePosition.Create(command.enterpriseId, command.name, command.description, command.isActive);
}
