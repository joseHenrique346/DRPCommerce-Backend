using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Carts;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Persistence;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SetMyDefaultAddressCommandHandler(
    ICustomerAddressRepository addresses,
    ICustomerRepository customers,
    CustomerAddressWriteScope writes,
    IUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider)
    : WorkflowRequestHandler<SetMyDefaultAddressCommand, List<CustomerAddressResponse>>
{
    protected override async Task<List<CustomerAddressResponse>> ExecuteAsync(SetMyDefaultAddressCommand command, CancellationToken ct)
    {
        var id = command.Id;
        var (enterpriseId, customerId) = (tenantProvider.GetEnterpriseId(), actor.CustomerId);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await customers.LockAsync(customerId, ct);

        var customer = await customers.FindAsync(item => item.Id == customerId && item.IsActive, ct, tracking: false)
            ?? throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");
        var book = new CustomerAddressBook(await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: true, ct));

        using (writes.Begin(enterpriseId, customerId))
        {
            book.MakeDefault(CustomerAddressSelection.Find(book, id));
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return (await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: false, ct)).Select(CustomerAddressResponse.FromEntity).ToList();
    }
}
