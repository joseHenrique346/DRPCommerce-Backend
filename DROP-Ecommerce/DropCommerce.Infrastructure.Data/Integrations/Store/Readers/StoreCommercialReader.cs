using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Infrastructure.Data.Integrations.Store.Clients;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Readers;

public sealed class StoreCommercialReader(IStoreEnterpriseRefitApi api, StoreIntegrationScope integration) : IStoreCommercialReader
{
    public async Task<DropCommercialRights> RightsAsync(long enterpriseId, CancellationToken ct)
    {
        using var operation = integration.Begin(enterpriseId);

        try
        {
            using var response = await api.DropContractAsync(enterpriseId, ct);
            var rights = StoreIntegrationResponseReader.ReadContentOrNull(response, "DROP contract");

            if (rights is null || rights.EnterpriseId != enterpriseId || rights.ContractRevision < 1 ||
                rights.CatalogVersion != 1 || rights.ProductLimit < 0 || !rights.AllowsDrop && rights.ProductLimit != 0)
                throw new InvalidOperationException("Contrato DROP inválido.");

            return rights;
        }
        catch (Exception exception) when (exception is not OperationCanceledException || !ct.IsCancellationRequested)
        {
            throw new SecurityFailure(503, "DropContract.Unavailable", "Não foi possível confirmar o contrato atual da loja.");
        }
    }
}
