using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeleteRoleCommand(long Id) : IIdentityRequest<bool>;
