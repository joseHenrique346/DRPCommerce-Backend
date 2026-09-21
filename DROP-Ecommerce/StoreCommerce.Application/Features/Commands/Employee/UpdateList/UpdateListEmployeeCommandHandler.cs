using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListEmployeeCommandHandler(IRepository<Employee> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateEmployeeCommand, UpdateListEmployeeCommand, Employee>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateEmployeeCommand> GetCommandList(UpdateListEmployeeCommand request) => request.commands;

    protected override long GetById(UpdateEmployeeCommand command) => command.id;

    protected override void ApplyChanges(Employee entity, UpdateEmployeeCommand command)
    {
        entity.UpdatePersonalInfo(command.fullName, command.email, command.roleId, command.departmentId);
        entity.UpdatePasswordHash(command.passwordHash);
    }
}
