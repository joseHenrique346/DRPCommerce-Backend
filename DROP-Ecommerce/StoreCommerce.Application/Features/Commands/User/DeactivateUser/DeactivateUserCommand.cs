using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeactivateUserCommand(DeleteAccountRequest Request) : IIdentityRequest<bool>;
