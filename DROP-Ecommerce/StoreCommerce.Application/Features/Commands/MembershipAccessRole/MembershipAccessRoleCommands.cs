using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateMembershipAccessRoleCommand(long enterpriseMembershipId, long accessRoleId) : IRequest<Result<MembershipAccessRoleResponse>>;
public record class DeleteMembershipAccessRoleCommand(long enterpriseMembershipId, long accessRoleId) : IRequest<Result<bool>>;
public record class GetByKeyMembershipAccessRoleQuery(long enterpriseMembershipId, long accessRoleId) : IRequest<Result<MembershipAccessRoleResponse>>;
public record class GetAllMembershipAccessRoleQuery : IRequest<Result<List<MembershipAccessRoleResponse>>>;
