using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SaveRoleCommand(long? Id, AccessRoleRequest Request) : IIdentityRequest<AccessRoleResponse>;
