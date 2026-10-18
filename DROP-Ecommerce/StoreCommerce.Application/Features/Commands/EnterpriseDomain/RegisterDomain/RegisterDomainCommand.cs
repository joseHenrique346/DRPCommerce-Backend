using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RegisterDomainCommand(DomainRegistrationRequest Request) : IIdentityRequest<DomainChallenge>;
