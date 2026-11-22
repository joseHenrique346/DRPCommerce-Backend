using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ValidateEnterpriseTeamSelectionQuery(
    long Tenant,
    long PlanId,
    long? AddOnId,
    IReadOnlyList<long>? Selection,
    bool Required) : IIdentityRequest<IReadOnlyList<long>?>;
