using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record InviteCommand(InvitationRequest Request) : IIdentityRequest<OneTimeLink>;
