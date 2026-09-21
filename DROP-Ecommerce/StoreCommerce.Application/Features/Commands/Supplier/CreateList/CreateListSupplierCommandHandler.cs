using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListSupplierCommandHandler(IRepository<Supplier> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateSupplierCommand, CreateListSupplierCommand, Supplier>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateSupplierCommand> GetCommandList(CreateListSupplierCommand request) => request.commands;

    protected override Supplier CreateEntity(CreateSupplierCommand command) =>
        Supplier.Create(command.enterpriseId, command.companyName, command.contactName, command.email, command.phone, command.addressLine, command.city, command.stateId, command.zipCode, command.country, command.isActive);
}
