using MediatR;
using StoreCommerce.Application.Contracts.Identity;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateAccessRolePermissionCommand(long accessRoleId, long permissionId) : IRequest<Result<AccessRolePermissionResponse>>;
