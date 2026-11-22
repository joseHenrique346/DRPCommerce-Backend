using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ValidateEnterpriseTeamSelectionQueryHandler(
    IEnterpriseAccessReader reader,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ValidateEnterpriseTeamSelectionQuery, IReadOnlyList<long>?>(context)
{
    protected override async Task<IReadOnlyList<long>?> ExecuteAsync(ValidateEnterpriseTeamSelectionQuery query, CancellationToken ct)
    {
        var rights = EnterpriseEntitlements.Resolve(query.PlanId, query.AddOnId);
        var active = TeamContractPolicy.ActiveAdditional(await reader.MembersAsync(query.Tenant, ct));

        if (!TeamContractPolicy.IsValidSelection(query.Selection, active, rights.EmployeeLimit))
            throw new SecurityFailure(400, "Team.InvalidSelection", "Selecione vínculos ativos da própria loja dentro do limite.");

        if (query.Required && active.Count > rights.EmployeeLimit && query.Selection is null)
            throw new SecurityFailure(409, "Team.SelectionRequired", "Escolha quem permanece na equipe antes de solicitar a mudança.");

        return query.Selection?.OrderBy(Id => Id).ToArray();
    }
}
