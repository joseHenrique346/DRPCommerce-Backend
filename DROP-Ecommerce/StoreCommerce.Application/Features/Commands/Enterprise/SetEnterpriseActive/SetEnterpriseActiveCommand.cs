using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SetEnterpriseActiveCommand(long EnterpriseId, bool Active) : IIdentityRequest<bool>;
