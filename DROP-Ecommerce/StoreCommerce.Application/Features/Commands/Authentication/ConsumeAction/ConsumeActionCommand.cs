using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ConsumeActionCommand(ConsumeActionRequest Request, AccountActionPurpose Purpose, bool Customer) : IIdentityRequest<bool>;
