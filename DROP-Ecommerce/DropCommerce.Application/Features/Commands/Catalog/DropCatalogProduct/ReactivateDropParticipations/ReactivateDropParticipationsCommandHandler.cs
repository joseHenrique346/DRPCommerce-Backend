using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Application.Interfaces.Identity;
using DropCommerce.Domain.Interfaces.Persistence;
using System.Text.Json;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ReactivateDropParticipationsCommandHandler(
    IDropCatalogRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    ICurrentDropActor actor)
    : IRequestHandler<ReactivateDropParticipationsCommand, Result<DropReactivationResponse>>,
      IRequestHandler<PreviewDropReactivationQuery, Result<DropReactivationResponse>>
{
    private long Tenant => actor.EnterpriseId;

    public Task<Result<DropReactivationResponse>> Handle(ReactivateDropParticipationsCommand command, CancellationToken ct)
    {
        return EvaluateAsync(command.Input, true, ct);
    }

    public Task<Result<DropReactivationResponse>> Handle(PreviewDropReactivationQuery query, CancellationToken ct)
    {
        return EvaluateAsync(query.Input, false, ct);
    }

    private async Task<Result<DropReactivationResponse>> EvaluateAsync(ReactivateDropParticipations request, bool confirm, CancellationToken ct)
    {
        Require("drop.events.manage");
        var tenant = Tenant;
        var rights = await persistence.RefreshCommercialRightsAsync(tenant, ct);

        if (!rights.AllowsDrop || request.ExpectedContractRevision != rights.ContractRevision)
            throw new SecurityFailure(409, "DropContract.RevisionChanged", "Atualize os direitos e o modo de reativação.");

        using var operation = writes.Begin("reactivation");
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(tenant, ct);
        var products = await persistence.ListActiveParticipationsAsync(ct);
        var used = products.Where(Product => !Product.IsContractBlocked && DropEventPublication.IsPublished(Product.DropEvent))
            .Select(Product => (Product.DropEventId, Product.CatalogProductId)).Distinct().Count();
        var available = Math.Max(0, rights.ProductLimit - used);
        var eligible = products.Where(Product => Product.IsContractBlocked && DropEventPublication.IsPublished(Product.DropEvent) &&
            Product.CatalogProduct is { IsDeleted: false, IsReturning: false, IsReturned: false } &&
            (!Product.CatalogProduct.StoreProductId.HasValue || Product.CatalogProduct.SourceExportId.HasValue)).ToList();
        var selected = request.Mode == "oldest" ? eligible.Take(available).ToList() :
            eligible.Where(Product => request.ParticipationIds?.Contains(Product.Id) == true).ToList();

        if (request.Mode == "selected" && (request.ParticipationIds is null || request.ParticipationIds.Distinct().Count() != request.ParticipationIds.Count ||
            selected.Count != request.ParticipationIds.Count) || selected.Count > available)
            throw new SecurityFailure(409, "DropContract.InvalidSelection", "Selecione participações elegíveis dentro das vagas disponíveis.");

        if (confirm)
        {
            foreach (var product in selected)
            {
                product.CatalogProduct!.Reactivate();
                product.ReactivateForContract();
                persistence.RecordCommercialActivity(rights, "participation_reactivated", product.CatalogProductId, product.Id);
            }

            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);
        }

        return Result<DropReactivationResponse>.Success(new DropReactivationResponse(rights.ContractRevision, available, selected.Select(Product => Product.Id).ToList(), confirm));
    }

    private void Require(string permission)
    {
        if (!actor.IsAuthenticated || !actor.IsUser || !actor.HasMembership || !actor.HasPermission(permission))
            throw new SecurityFailure(403, "DropCatalog.PermissionRequired", "Permissão de gestão DROP necessária.");
    }
}
