using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListEmployeeCommandHandler(IRepository<Employee> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateEmployeeCommand, CreateListEmployeeCommand, Employee>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateEmployeeCommand> GetCommandList(CreateListEmployeeCommand request) => request.commands;

    protected override Employee CreateEntity(CreateEmployeeCommand command) =>
        Employee.Create(command.enterpriseId, command.fullName, command.email, command.passwordHash, command.roleId, command.departmentId, command.isActive, command.hiredAt);
}
