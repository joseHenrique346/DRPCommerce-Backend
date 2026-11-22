using StoreCommerce.Application.Interfaces.StoreManagement;
using SharedCommerce.Application.Result;
using MediatR;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class PreviewAccessContractImpactQueryHandler(
    IdentityHandlerContext context, IEnterpriseAccessReader reader, IMediator mediator)
    : EnterpriseContractHandlerBase<PreviewAccessContractImpactQuery, AccessContractImpactResponse>(context)
{
    protected override async Task<AccessContractImpactResponse> ExecuteAsync(PreviewAccessContractImpactQuery command, CancellationToken ct)
    {
        var request = command.Request;

        if (request is null || request.CatalogVersion != EnterpriseCommercialCatalog.Version ||
            EnterprisePlan.Find(request.PlanId) is null || request.AddOnId.HasValue &&
            (EnterpriseAddOn.Find(request.AddOnId.Value) is null || !EnterpriseAddOn.FromId(request.AddOnId.Value).IsEligibleFor(request.PlanId)))
            throw new SecurityFailure(400, "Contract.InvalidOffer", "Atualize o catálogo e escolha uma oferta válida.");

        using var operation = scope.Begin();
        await using var transaction = await BeginOwnerOperationAsync(ct);
        var state = await ReadCurrentContractAsync(ct);

        if (state.Revision != request.ExpectedContractRevision)
            throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato mudou. Atualize a consulta.");

        var selection = await mediator.SendRequiredAsync(new ValidateEnterpriseTeamSelectionQuery(actor.EnterpriseId, request.PlanId, request.AddOnId,
            request.RetainedMembershipIds, false), ct);
        var members = await reader.MembersAsync(actor.EnterpriseId, ct);
        var active = TeamContractPolicy.ActiveAdditional(members);
        var domains = await reader.DomainsAsync(actor.EnterpriseId, ct);
        var rights = EnterpriseEntitlements.Resolve(request.PlanId, request.AddOnId);
        var requires = active.Count > rights.EmployeeLimit;
        var platform = domains.FirstOrDefault(Domain => DomainHostPolicy.IsPlatformHost(Domain.NormalizedHost, Options.StorefrontBaseDomain));
        var result = new AccessContractImpactResponse(actor.EnterpriseId, state.Revision, request.PlanId, request.AddOnId,
            rights.EmployeeLimit, active.Count, members.Count(Membership => Membership.IsContractBlocked), requires,
            active.Select(Membership => Membership.Id).ToList(), selection,
            requires && selection is not null ? active.Where(Membership => !selection.Contains(Membership.Id))
                .Select(Membership => Membership.Id).ToList() : [],
            rights.AllowsCustomDomain, rights.AllowsCustomDomain ? [] : domains.Where(Domain =>
                !Domain.IsContractBlocked && !DomainHostPolicy.IsPlatformHost(Domain.NormalizedHost, Options.StorefrontBaseDomain))
                .Select(Domain => Domain.Id).ToList(), platform?.Host);
        await transaction.CommitAsync(ct);

        return result;
    }
}
