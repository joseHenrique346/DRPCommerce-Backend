using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Domain.Enums.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RequestActionCommand(AccountActionRequest Request, AccountActionPurpose Purpose, bool Customer) : IIdentityRequest<bool>;
