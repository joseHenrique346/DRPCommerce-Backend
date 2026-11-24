using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetEnterpriseContractChangeQuery(long Id)
    : IIdentityRequest<EnterpriseContractChangeResponse>;
