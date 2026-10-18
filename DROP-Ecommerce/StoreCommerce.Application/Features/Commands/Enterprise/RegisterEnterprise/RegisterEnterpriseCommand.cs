using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RegisterEnterpriseCommand(EnterpriseRegistrationRequest Request) : IIdentityRequest<SafeEnterprise>;
