using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetMembershipRoleCommand(long MembershipId, long RoleId, bool Grant) : IIdentityRequest<bool>;
