using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record LogoutAllCommand(bool Customer) : IIdentityRequest<bool>;
