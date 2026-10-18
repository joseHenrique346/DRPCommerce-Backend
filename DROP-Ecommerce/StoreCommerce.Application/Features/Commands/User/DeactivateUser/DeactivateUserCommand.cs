using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeactivateUserCommand() : IIdentityRequest<bool>;
