using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class GetByKeyMembershipAccessRoleQuery(long enterpriseMembershipId, long accessRoleId) : IRequest<Result<MembershipAccessRoleResponse>>;
