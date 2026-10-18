using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListEnterprisesQuery() : IIdentityRequest<IReadOnlyList<MembershipChoice>>;
