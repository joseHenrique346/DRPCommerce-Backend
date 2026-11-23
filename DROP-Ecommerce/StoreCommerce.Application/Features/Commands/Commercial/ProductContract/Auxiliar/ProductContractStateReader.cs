using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Interfaces.StoreManagement;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ProductContractStateReader(
    CatalogWriteContext context,
    IEnterpriseRepository enterprises)
{
    public async Task<EnterpriseContractState> ReadAsync(long expectedRevision, CancellationToken ct)
    {
        var state = await enterprises.GetContractStateAsync(context.Tenant, ct)
            ?? throw new SecurityFailure(404, "Enterprise.Unavailable", "Loja indisponível.");

        if (expectedRevision <= 0 || state.Revision != expectedRevision)
            throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato mudou. Atualize a consulta.");

        return state;
    }
}
