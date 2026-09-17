using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCustomerCommandHandler(IRepository<Customer> repository, IUnitOfWork unitOfWork)
    : BaseCreateListHandler<CreateCustomerCommand, CreateListCustomerCommand, Customer>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCustomerCommand> GetCommandList(CreateListCustomerCommand request) => request.commands;

    protected override Customer CreateEntity(CreateCustomerCommand command) =>
        Customer.Create(command.enterpriseId, command.fullName, command.passwordHash, command.addressLine, command.city, 0, command.zipCode, command.country, command.gender, command.dateOfBirth, command.isVerified, command.isActive);
}
