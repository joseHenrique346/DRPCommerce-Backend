using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListPendingDropContractsQuery() : IIdentityRequest<IReadOnlyList<DropCommercialRights>>;
