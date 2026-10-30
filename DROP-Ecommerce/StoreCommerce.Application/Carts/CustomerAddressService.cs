using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts.Customers;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Carts;

/// <summary>
/// Endereços do cliente autenticado na vitrine. As alterações bloqueiam o cliente na transação, para o limite de endereços
/// e o endereço padrão continuarem corretos com requisições simultâneas.
/// </summary>
public sealed class CustomerAddressService(
    ICustomerAddressRepository addresses,
    ICustomerRepository customers,
    CustomerAddressWriteScope writes,
    IUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider)
{
    public const string DefaultCountry = Shipment.DefaultCountry;

    public async Task<List<CustomerAddressResponse>> ListAsync(CancellationToken ct)
    {
        var (enterpriseId, customerId) = (tenantProvider.GetEnterpriseId(), actor.CustomerId);

        return (await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: false, ct)).Select(CustomerAddressResponse.FromEntity).ToList();
    }

    public Task<List<CustomerAddressResponse>> AddAsync(CustomerAddressRequest request, CancellationToken ct) =>
        WriteAsync((customer, book) =>
        {
            var address = CustomerAddress.Create(customer, request.Label ?? string.Empty, request.RecipientName ?? string.Empty, request.AddressLine,
                request.City, request.StateId, request.ZipCode, Country(request));
            book.Add(address, request.IsDefault);
            addresses.Add(address);
        }, ct);

    public Task<List<CustomerAddressResponse>> UpdateAsync(long id, CustomerAddressRequest request, CancellationToken ct) =>
        WriteAsync((_, book) =>
        {
            var address = Find(book, id);
            address.UpdateDetails(request.Label ?? string.Empty, request.RecipientName ?? string.Empty, request.AddressLine, request.City,
                request.StateId, request.ZipCode, Country(request));

            if (request.IsDefault)
                book.MakeDefault(address);
        }, ct);

    public Task<List<CustomerAddressResponse>> SetDefaultAsync(long id, CancellationToken ct) =>
        WriteAsync((_, book) => book.MakeDefault(Find(book, id)), ct);

    public Task<List<CustomerAddressResponse>> DeleteAsync(long id, CancellationToken ct) =>
        WriteAsync((_, book) =>
        {
            var address = Find(book, id);
            book.Remove(address);
            addresses.Remove(address);
        }, ct);

    private async Task<List<CustomerAddressResponse>> WriteAsync(Action<Customer, CustomerAddressBook> change, CancellationToken ct)
    {
        var (enterpriseId, customerId) = (tenantProvider.GetEnterpriseId(), actor.CustomerId);

        await using var transaction = await unitOfWork.BeginTransactionAsync(ct);
        await customers.LockAsync(customerId, ct);

        var customer = await customers.FindAsync(item => item.Id == customerId && item.IsActive, ct, tracking: false)
            ?? throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");
        var book = new CustomerAddressBook(await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: true, ct));

        using (writes.Begin(enterpriseId, customerId))
        {
            change(customer, book);
            await unitOfWork.CommitAsync(ct);
        }

        await transaction.CommitAsync(ct);

        return (await addresses.ListForCustomerAsync(enterpriseId, customerId, tracking: false, ct)).Select(CustomerAddressResponse.FromEntity).ToList();
    }

    private static CustomerAddress Find(CustomerAddressBook book, long id) =>
        book.Addresses.FirstOrDefault(address => address.Id == id)
            ?? throw new SecurityFailure(404, "CustomerAddress.NotFound", "Endereço não encontrado.");

    private static string Country(CustomerAddressRequest request) =>
        string.IsNullOrWhiteSpace(request.Country) ? DefaultCountry : request.Country;
}
