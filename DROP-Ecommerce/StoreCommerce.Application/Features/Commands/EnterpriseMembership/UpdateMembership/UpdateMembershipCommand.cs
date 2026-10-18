using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateMembershipCommand(long Id, MembershipUpdateRequest Request) : IIdentityRequest<EnterpriseMembershipResponse>;
