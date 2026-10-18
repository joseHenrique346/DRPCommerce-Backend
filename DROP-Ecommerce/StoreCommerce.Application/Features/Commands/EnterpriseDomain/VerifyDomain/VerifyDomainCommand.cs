using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record VerifyDomainCommand(long Id) : IIdentityRequest<bool>;
