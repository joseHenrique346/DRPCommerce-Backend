namespace StoreCommerce.Application.Identity;

public interface IAuthenticationSessionRepository
{
    Task RevokeFamilyAsync(long accountId, Guid familyId, bool customer, DateTime revokedAt, CancellationToken cancellationToken);
    Task RevokeAllAsync(long accountId, bool customer, DateTime revokedAt, CancellationToken cancellationToken);
    Task<IReadOnlyList<SessionDescription>> ListAsync(long accountId, Guid currentFamilyId, bool customer, long? enterpriseId, DateTime now, CancellationToken cancellationToken);
    Task CleanupAsync(DateTime before, CancellationToken cancellationToken);
}
