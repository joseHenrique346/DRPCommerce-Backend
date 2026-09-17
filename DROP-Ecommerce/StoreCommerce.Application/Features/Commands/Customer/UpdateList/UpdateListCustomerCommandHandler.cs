using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class UpdateListCustomerCommandHandler(IRepository<Customer> repository, IUnitOfWork unitOfWork)
    : BaseUpdateListHandler<UpdateCustomerCommand, UpdateListCustomerCommand, Customer>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<UpdateCustomerCommand> GetCommandList(UpdateListCustomerCommand request) => request.commands;

    protected override long GetById(UpdateCustomerCommand command) => command.id;

    protected override void ApplyChanges(Customer entity, UpdateCustomerCommand command)
    {
        entity.UpdatePersonalInfo(command.fullName, command.gender, command.dateOfBirth);
        entity.UpdateAddress(command.addressLine, command.city, 0, command.zipCode, command.country);
        entity.UpdatePasswordHash(command.passwordHash);
    }
}
