using MediatR;
using StoreCommerce.Application.Contracts.Store;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class GetPublicStoreInfoQueryHandler(
    IRepository<Enterprise> enterpriseRepository,
    StoreSettingsReader reader,
    ITenantProvider tenantProvider)
    : IRequestHandler<GetPublicStoreInfoQuery, Result<PublicStoreInfoResponse>>
{
    public async Task<Result<PublicStoreInfoResponse>> Handle(GetPublicStoreInfoQuery request, CancellationToken cancellationToken)
    {
        var enterpriseId = tenantProvider.GetEnterpriseId();

        // Loja inativa não é exibida na vitrine.
        var store = await enterpriseRepository.FindValueAsync(
            enterprise => enterprise.Id == enterpriseId && enterprise.IsActive,
            enterprise => new { enterprise.TradeName, Email = enterprise.Email.Value, Phone = enterprise.Phone.Value },
            cancellationToken);

        if (store is null)
            return Result<PublicStoreInfoResponse>.NotFound("Store.Unavailable", "Loja indisponível.");

        var settings = await reader.GetCurrentAsync(cancellationToken);

        return Result<PublicStoreInfoResponse>.Success(new PublicStoreInfoResponse(
            store.TradeName,
            store.Email,
            store.Phone,
            settings.CurrencyCode,
            settings.MinOrderValue,
            settings.MaxItemsPerOrder));
    }
}
