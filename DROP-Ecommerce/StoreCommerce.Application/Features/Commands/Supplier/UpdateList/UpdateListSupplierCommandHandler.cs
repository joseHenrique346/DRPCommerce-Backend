using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListSupplierCommandHandler(IRepository<Supplier> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateSupplierCommand, UpdateListSupplierCommand, Supplier>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateSupplierCommand> GetCommandList(UpdateListSupplierCommand request) => request.commands;

    protected override long GetById(UpdateSupplierCommand command) => command.id;

    protected override void ApplyChanges(Supplier entity, UpdateSupplierCommand command)
    {
        entity.UpdateInfo(command.companyName, command.contactName);
        entity.UpdateAddress(command.addressLine, command.city, 0, command.zipCode, command.country);
    }
}
