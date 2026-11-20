using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class AccessContractPolicyService(IEnterpriseAccessReader reader)
{
    public async Task<IReadOnlyList<long>?> ValidateSelectionAsync(long tenant, long planId, long? addOnId,
        IReadOnlyList<long>? selection, bool required, CancellationToken ct)
    {
        var rights = EnterpriseEntitlements.Resolve(planId, addOnId);
        var active = TeamContractPolicy.ActiveAdditional(await reader.MembersAsync(tenant, ct));

        if (!TeamContractPolicy.IsValidSelection(selection, active, rights.EmployeeLimit))
            throw new SecurityFailure(400, "Team.InvalidSelection", "Selecione vínculos ativos da própria loja dentro do limite.");

        if (required && active.Count > rights.EmployeeLimit && selection is null)
            throw new SecurityFailure(409, "Team.SelectionRequired", "Escolha quem permanece na equipe antes de solicitar a mudança.");

        return selection?.OrderBy(Id => Id).ToArray();
    }
}
