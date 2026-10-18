using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record LoginCustomerCommand(LoginRequest Request) : IIdentityRequest<SessionResult>;
