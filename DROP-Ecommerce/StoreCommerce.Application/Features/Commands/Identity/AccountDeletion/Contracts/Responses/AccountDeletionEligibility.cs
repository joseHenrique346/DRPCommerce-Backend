namespace StoreCommerce.Application.Identity;

public sealed record AccountDeletionEligibility(bool CanDelete, IReadOnlyList<AccountDeletionBlocker> Blockers);
