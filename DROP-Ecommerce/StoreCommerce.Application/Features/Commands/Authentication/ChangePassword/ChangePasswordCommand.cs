using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ChangePasswordCommand(ChangePasswordRequest Request, bool Customer) : IIdentityRequest<bool>;
