using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetDropCommercialRightsQuery(long Tenant) : IIdentityRequest<DropCommercialRights?>;
