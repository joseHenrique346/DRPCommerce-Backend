using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListDepartmentCommandHandler(IRepository<Department> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateDepartmentCommand, UpdateListDepartmentCommand, Department>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateDepartmentCommand> GetCommandList(UpdateListDepartmentCommand request) => request.commands;

    protected override long GetById(UpdateDepartmentCommand command) => command.id;

    protected override void ApplyChanges(Department entity, UpdateDepartmentCommand command)
    {
        entity.UpdateDetails(command.name, command.description);
    }
}
