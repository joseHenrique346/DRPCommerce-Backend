using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetDomainActiveCommand(long Id, DomainActivationRequest Request) : IIdentityRequest<bool>;
