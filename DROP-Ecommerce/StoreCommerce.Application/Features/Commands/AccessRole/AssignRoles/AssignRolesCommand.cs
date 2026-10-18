using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AssignRolesCommand(long MembershipId, MembershipRolesRequest Request) : IIdentityRequest<bool>;
