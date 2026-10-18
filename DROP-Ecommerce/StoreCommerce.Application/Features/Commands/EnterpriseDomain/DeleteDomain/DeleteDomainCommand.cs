using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeleteDomainCommand(long Id) : IIdentityRequest<bool>;
