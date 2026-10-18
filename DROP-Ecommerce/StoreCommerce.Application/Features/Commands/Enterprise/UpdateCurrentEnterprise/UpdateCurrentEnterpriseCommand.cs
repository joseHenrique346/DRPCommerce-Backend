using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record UpdateCurrentEnterpriseCommand(EnterpriseUpdateRequest Request) : IIdentityRequest<SafeEnterprise>;
