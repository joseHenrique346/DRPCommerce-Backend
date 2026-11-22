using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Interfaces.Identity;

public interface IAccountDeletionStore
{
    Task<AccountDeletionEligibility> InspectUserAsync(long userId, CancellationToken ct);
    Task<AccountDeletionEligibility> InspectCustomerAsync(long enterpriseId, long customerId, DateTime now, CancellationToken ct);
    Task EraseUserRelatedDataAsync(long userId, string normalizedEmail, DateTime now, CancellationToken ct);
    Task EraseCustomerRelatedDataAsync(long enterpriseId, long customerId, CancellationToken ct);
}
