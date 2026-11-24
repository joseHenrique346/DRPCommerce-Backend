using DropCommerce.Application.Contracts;
using DropCommerce.Application.Interfaces.Catalog;
using DropCommerce.Domain.Entity;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;

namespace DropCommerce.Application.Features.Commands;

public sealed class DropReactivationSelector(
    IDropCatalogRepository persistence)
{
    public async Task<DropReactivationSelection> SelectAsync(
        ReactivateDropParticipations request, DropCommercialRights rights, CancellationToken ct)
    {
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

        return new DropReactivationSelection(available, selected);
    }
}
