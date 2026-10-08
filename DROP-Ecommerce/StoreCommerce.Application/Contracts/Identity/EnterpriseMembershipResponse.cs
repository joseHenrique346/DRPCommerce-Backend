namespace StoreCommerce.Application.Contracts.Identity;

public sealed record EnterpriseMembershipResponse(long Id, long EnterpriseId, long UserId, long KindId, bool IsActive, DateTime JoinedAt);
