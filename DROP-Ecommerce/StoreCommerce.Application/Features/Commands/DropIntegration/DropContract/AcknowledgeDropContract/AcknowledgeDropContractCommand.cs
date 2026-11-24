using StoreCommerce.Application.Interfaces.Identity;
using SharedCommerce.Application.Contracts.Drop;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record AcknowledgeDropContractCommand(long Tenant, long Revision) : IIdentityRequest<bool>;
