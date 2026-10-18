using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record LogoutCommand(string Token, bool Customer) : IIdentityRequest<bool>;
