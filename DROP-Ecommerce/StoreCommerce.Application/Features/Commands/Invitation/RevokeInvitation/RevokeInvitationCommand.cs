using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RevokeInvitationCommand(long Id) : IIdentityRequest<bool>;
