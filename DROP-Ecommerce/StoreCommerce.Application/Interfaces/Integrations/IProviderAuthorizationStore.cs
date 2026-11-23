using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Interfaces.Integrations;

public interface IProviderAuthorizationStore
{
    Task<EnterpriseMembership?> FindMembershipAsync(long enterpriseId, long userId, long membershipId, CancellationToken ct);
    Task<bool> HasSettingsPermissionAsync(long enterpriseId, long membershipId, CancellationToken ct);
    Task<ProviderAuthorization?> FindAuthorizationAsync(string hash, string browserHash, DateTime now, CancellationToken ct);
    Task<int> ConsumeAuthorizationAsync(long authorizationId, DateTime now, CancellationToken ct);
    void Add(ProviderAuthorization authorization);
    Task<IIdentityTransaction> BeginSerializableTransactionAsync(CancellationToken ct);
}
