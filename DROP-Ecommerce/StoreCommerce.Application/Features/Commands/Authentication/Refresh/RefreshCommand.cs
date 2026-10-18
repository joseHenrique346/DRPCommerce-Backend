using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RefreshCommand(string Token, bool Customer, long? MembershipId) : IIdentityRequest<SessionResult>;
