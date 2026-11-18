using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DeactivateCustomerAccountCommand(DeleteAccountRequest Request) : IIdentityRequest<bool>;
