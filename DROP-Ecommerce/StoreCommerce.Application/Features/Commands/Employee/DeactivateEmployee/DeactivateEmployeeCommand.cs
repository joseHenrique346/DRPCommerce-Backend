using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeactivateEmployeeCommand(long Id) : IIdentityRequest<bool>;
