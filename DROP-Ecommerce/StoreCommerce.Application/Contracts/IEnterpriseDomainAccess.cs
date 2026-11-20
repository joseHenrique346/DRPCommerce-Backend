namespace StoreCommerce.Application.Contracts;

public interface IEnterpriseDomainAccess
{
    Task<long?> ResolveAsync(string host, long? enterpriseId, CancellationToken ct);
}
