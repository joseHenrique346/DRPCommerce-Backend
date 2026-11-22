using StoreCommerce.Application.Contracts;

namespace StoreCommerce.Application.Interfaces.StoreManagement;

public interface IEnterpriseDomainAccess
{
    Task<long?> ResolveAsync(string host, long? enterpriseId, CancellationToken ct);
}
