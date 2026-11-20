using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record PreviewAccessContractImpactQuery(AccessContractImpactRequest Request)
    : IIdentityRequest<AccessContractImpactResponse>;
