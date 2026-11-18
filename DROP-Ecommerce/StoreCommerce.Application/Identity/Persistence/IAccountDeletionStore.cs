namespace StoreCommerce.Application.Identity;

public sealed record AccountDeletionBlocker(string Code, string Message, int Count);
public sealed record AccountDeletionEligibility(bool CanDelete, IReadOnlyList<AccountDeletionBlocker> Blockers);

public interface IAccountDeletionStore
{
    Task<AccountDeletionEligibility> InspectUserAsync(long userId, CancellationToken ct);
    Task<AccountDeletionEligibility> InspectCustomerAsync(long enterpriseId, long customerId, DateTime now, CancellationToken ct);
    Task EraseUserRelatedDataAsync(long userId, string normalizedEmail, DateTime now, CancellationToken ct);
    Task EraseCustomerRelatedDataAsync(long enterpriseId, long customerId, CancellationToken ct);
}
