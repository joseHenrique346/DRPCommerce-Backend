using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class CartPurchaseContextReader(
    ICustomerRepository customers,
    StoreSettingsReader storeSettings,
    CategoryCatalogReader categoryReader,
    ICurrentIdentity actor,
    ITenantProvider tenantProvider)
{
    public async Task<CartPurchaseContext> ReadAsync(CancellationToken ct)
    {
        var customerId = actor.CustomerId;
        var enterpriseId = tenantProvider.GetEnterpriseId();
        var categories = await categoryReader.ReadAsync(storefront: true, ct);

        if (categories.IsFailure)
            throw new SecurityFailure(404, "Store.Unavailable", "Loja indisponível.");

        if (!await customers.ExistsAsync(customer => customer.Id == customerId && customer.IsActive, ct))
            throw new SecurityFailure(401, "Identity.InvalidCredentials", "Credenciais inválidas ou acesso temporariamente indisponível.");

        return new CartPurchaseContext(enterpriseId, customerId, await storeSettings.GetCurrentAsync(ct),
            categories.Content!.Select(category => category.Id).ToHashSet());
    }
}
