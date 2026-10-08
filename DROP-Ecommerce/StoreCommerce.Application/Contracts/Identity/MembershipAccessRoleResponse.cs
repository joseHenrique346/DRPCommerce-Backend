namespace StoreCommerce.Application.Contracts.Identity;

public sealed record MembershipAccessRoleResponse(long EnterpriseId, long EnterpriseMembershipId, long AccessRoleId);
