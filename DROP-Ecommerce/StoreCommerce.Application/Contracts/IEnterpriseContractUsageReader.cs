namespace StoreCommerce.Application.Contracts;

public interface IEnterpriseContractUsageReader
{
    Task<EnterpriseContractUsage> ReadAsync(long enterpriseId, CancellationToken cancellationToken);
}
