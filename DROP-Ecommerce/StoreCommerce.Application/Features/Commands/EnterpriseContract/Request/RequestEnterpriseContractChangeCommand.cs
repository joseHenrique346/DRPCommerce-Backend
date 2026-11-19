using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RequestEnterpriseContractChangeCommand(RequestEnterpriseContractChangeRequest Request)
    : IIdentityRequest<EnterpriseContractChangeResponse>;
