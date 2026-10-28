using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class CreateListCustomerCommandHandler(IRepository<Customer> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : BaseCreateListHandler<CreateCustomerCommand, CreateListCustomerCommand, Customer>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<CreateCustomerCommand> GetCommandList(CreateListCustomerCommand request) => request.commands;

    protected override Customer CreateEntity(CreateCustomerCommand command) =>
        Customer.Create(tenantProvider.GetEnterpriseId(), command.fullName, CustomerEmail.Create(command.email), CustomerPhone.Create(command.phone), command.addressLine, command.city, command.stateId, command.zipCode, command.country, command.gender, command.dateOfBirth, command.isVerified, command.isActive);
}
