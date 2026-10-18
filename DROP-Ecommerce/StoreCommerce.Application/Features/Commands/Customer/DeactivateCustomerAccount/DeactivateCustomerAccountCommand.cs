using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeactivateCustomerAccountCommand() : IIdentityRequest<bool>;
