using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ChangeEnterprisePlanCommand(long PlanId) : IIdentityRequest<bool>;
