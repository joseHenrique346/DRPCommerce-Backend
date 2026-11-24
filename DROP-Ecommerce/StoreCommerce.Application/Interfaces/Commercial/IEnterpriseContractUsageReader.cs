using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Interfaces.Commercial;

public interface IEnterpriseContractUsageReader
{
    Task<EnterpriseContractUsage> ReadAsync(long enterpriseId, CancellationToken cancellationToken);
}
