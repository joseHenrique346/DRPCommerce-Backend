using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateAccessRolePermissionCommand(long enterpriseId, long accessRoleId, long permissionId) : IRequest<Result<AccessRolePermissionResponse>>;
public record class DeleteAccessRolePermissionCommand(long accessRoleId, long permissionId) : IRequest<Result<bool>>;
public record class GetByKeyAccessRolePermissionQuery(long accessRoleId, long permissionId) : IRequest<Result<AccessRolePermissionResponse>>;
public record class GetAllAccessRolePermissionQuery : IRequest<Result<List<AccessRolePermissionResponse>>>;
