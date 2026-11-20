using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RequestEnterpriseContractChangeCommandHandler(
    IdentityHandlerContext context,
    IEntityRepository<EnterpriseContractChange> changes,
    AccessContractPolicyService accessPolicy)
    : EnterpriseContractHandlerBase<RequestEnterpriseContractChangeCommand, EnterpriseContractChangeResponse>(context)
{
    protected override async Task<EnterpriseContractChangeResponse> ExecuteAsync(
        RequestEnterpriseContractChangeCommand command, CancellationToken ct)
    {
        var request = command.Request;

        if (request is null || string.IsNullOrWhiteSpace(request.IdempotencyKey) ||
            request.IdempotencyKey.Trim().Length > 100 || request.ExpectedContractRevision <= 0)
            throw new SecurityFailure(400, "Contract.InvalidRequest", "Solicitação de contrato inválida.");

        if (request.RetainedMembershipIds is not null && (request.RetainedMembershipIds.Count > 10000 ||
            request.RetainedMembershipIds.Any(Id => Id <= 0) ||
            request.RetainedMembershipIds.Distinct().Count() != request.RetainedMembershipIds.Count))
            throw new SecurityFailure(400, "Team.InvalidSelection", "Seleção de equipe inválida.");

        using var operation = scope.Begin();
        await using var transaction = await BeginOwnerOperationAsync(ct);
        var tenant = actor.EnterpriseId;
        var key = request.IdempotencyKey.Trim();
        var existing = await changes.FindAsync(Change =>
            Change.EnterpriseId == tenant && Change.IdempotencyKey == key, ct,
            ignoreQueryFilters: true, tracking: false);

        if (existing is not null)
        {
            if (existing.PlanId != request.PlanId || existing.AddOnId != request.AddOnId ||
                existing.ExpectedContractRevision != request.ExpectedContractRevision ||
                existing.CatalogVersion != request.CatalogVersion ||
                (existing.RetainedMembershipIds is null) != (request.RetainedMembershipIds is null) ||
                existing.RetainedMembershipIds is not null &&
                    !existing.RetainedMembershipIds.SequenceEqual(request.RetainedMembershipIds!.OrderBy(Id => Id)))
                throw new SecurityFailure(409, "Contract.IdempotencyConflict",
                    "A chave já foi utilizada com outra solicitação.");

            await transaction.CommitAsync(ct);

            return EnterpriseContractResponseMapper.Change(existing);
        }

        if (request.CatalogVersion != EnterpriseCommercialCatalog.Version)
            throw new SecurityFailure(409, "Contract.CatalogChanged", "Atualize o catálogo antes de confirmar a escolha.");

        if (EnterprisePlan.Find(request.PlanId) is null ||
            request.AddOnId.HasValue && EnterpriseAddOn.Find(request.AddOnId.Value) is null)
            throw new SecurityFailure(400, "Contract.InvalidOffer", "Plano ou adicional inexistente.");

        if (request.AddOnId.HasValue && !EnterpriseAddOn.FromId(request.AddOnId.Value).IsEligibleFor(request.PlanId))
            throw new SecurityFailure(400, "Contract.IncompatibleAddOn", "O adicional não está disponível para este plano.");

        var current = await data.Enterprises.GetContractStateAsync(tenant, ct) ?? throw NotFound();

        if (current.Revision != request.ExpectedContractRevision)
            throw new SecurityFailure(409, "Contract.RevisionChanged", "O contrato mudou. Consulte os direitos atuais.");

        var selection = await accessPolicy.ValidateSelectionAsync(tenant, request.PlanId, request.AddOnId,
            request.RetainedMembershipIds, true, ct);

        var now = Now;
        var pending = await changes.FindAsync(Change =>
            Change.EnterpriseId == tenant && Change.Status == EnterpriseContractChangeStatus.Requested,
            ct, ignoreQueryFilters: true);

        if (pending is not null)
        {
            pending.Supersede(now);
            await unitOfWork.CommitAsync(ct);
        }

        var change = EnterpriseContractChange.Request(
            tenant, actor.UserId, key, request.PlanId, request.AddOnId, current.Revision, now, selection);
        changes.Add(change);
        await unitOfWork.CommitAsync(ct);
        await transaction.CommitAsync(ct);

        return EnterpriseContractResponseMapper.Change(change);
    }
}
