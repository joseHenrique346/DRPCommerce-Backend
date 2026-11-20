namespace StoreCommerce.Application.Contracts;

public sealed record AccessContractImpactResponse(
    long EnterpriseId, long ContractRevision, long TargetPlanId, long? TargetAddOnId,
    int AdditionalMemberLimit, int ActiveAdditionalMembers, int BlockedAdditionalMembers,
    bool RequiresMembershipSelection, IReadOnlyList<long> EligibleMembershipIds,
    IReadOnlyList<long>? RetainedMembershipIds, IReadOnlyList<long> MembershipIdsToBlock,
    bool AllowsCustomDomain, IReadOnlyList<long> DomainIdsToBlock, string? PlatformHost);
