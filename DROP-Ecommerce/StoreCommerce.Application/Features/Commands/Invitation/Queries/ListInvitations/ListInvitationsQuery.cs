using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListInvitationsQuery() : IIdentityRequest<IReadOnlyList<InvitationSummary>>;
