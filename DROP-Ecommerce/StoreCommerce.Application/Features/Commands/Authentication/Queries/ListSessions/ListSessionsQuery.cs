using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListSessionsQuery(bool Customer) : IIdentityRequest<IReadOnlyList<SessionDescription>>;
