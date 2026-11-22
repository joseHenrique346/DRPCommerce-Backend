namespace StoreCommerce.Application.Contracts;

public sealed record PlatformBillingTeamSelection(long ExpectedContractRevision, IReadOnlyList<long>? RetainedMembershipIds);
