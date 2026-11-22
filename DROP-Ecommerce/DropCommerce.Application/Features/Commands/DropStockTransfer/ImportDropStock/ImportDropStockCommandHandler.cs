using System.Text.Json;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ImportDropStockCommandHandler(
    IDropStockSagaRepository persistence,
    IUnitOfWork unitOfWork,
    DropCommercialWriteScope writes,
    IStoreCommercialReader contracts)
    : IRequestHandler<ImportDropStockCommand, Result<DropStockImportReceipt>>
{
    public async Task<Result<DropStockImportReceipt>> Handle(ImportDropStockCommand command, CancellationToken ct)
    {
        DropStockExport export = command.Export;

        var rights = await contracts.RightsAsync(export.EnterpriseId, ct);
        await persistence.ApplyCommercialRightsAsync(rights, ct);
        using var operation = writes.Begin("transfer", export.EnterpriseId);
        await using var tx = await persistence.BeginTransactionAsync(ct);
        await persistence.LockCommercialTenantAsync(export.EnterpriseId, ct);
        var receipt = await persistence.FindReceiptAsync(export.OperationId, ct);

        if (receipt is not null)
        {
            if (receipt.EnterpriseId != export.EnterpriseId)
                throw new SecurityFailure(409, "DropStock.TenantMismatch", "Transferência de outra loja.");

            return Result<DropStockImportReceipt>.Success(receipt);
        }

        var product = await persistence.FindSourceProductAsync(export.EnterpriseId, export.ProductId, ct);
        var total = await persistence.CountCatalogAsync(export.EnterpriseId, ct);
        string? rejection = !rights.AllowsDrop ? "DropContract.ModuleRequired" :
            product is null && total >= rights.ProductLimit ? "DropContract.CreationLimit" :
            product?.IsDeleted == true ? "DropCatalog.Deleted" :
            product is not null && export.Variants.Any(Source => product.Variants.Any(Variant =>
                Variant.SKU == Source.SKU && Variant.SourceVariantId.HasValue && Variant.SourceVariantId != Source.SourceVariantId))
                ? "DropStock.VariantMappingConflict" : null;

        if (rejection is not null)
        {
            receipt = DropStockImportReceipt.Record(export.OperationId, export.EnterpriseId, null, rejection);
            persistence.Add(receipt);
            await unitOfWork.CommitAsync(ct);
            await tx.CommitAsync(ct);

            return Result<DropStockImportReceipt>.Success(receipt);
        }

        if (product is null)
        {
            product = DropCatalogProduct.Create(export.EnterpriseId, export.Name, export.Description);
            persistence.Add(product);
        }

        product.Import(export.ProductId, export.OperationId, export.Name, export.Description,
            export.Variants.Select(Variant => Variant.SourceVariantId).ToArray(), export.OriginalCreatedAt);

        foreach (var source in export.Variants)
        {
            var variant = product.Variants.SingleOrDefault(Variant => Variant.SourceVariantId == source.SourceVariantId) ??
                product.Variants.SingleOrDefault(Variant => Variant.SKU == source.SKU && !Variant.SourceVariantId.HasValue);

            if (variant is null)
            {
                variant = DropCatalogVariant.Create(product, source.SKU, source.OptionsJson, source.Price, 0);
                product.Variants.Add(variant);
            }

            variant.Import(source.SourceVariantId, source.SKU, source.OptionsJson, source.Price, source.Quantity);
        }

        receipt = DropStockImportReceipt.Record(export.OperationId, export.EnterpriseId, product, null);
        persistence.Add(receipt);
        await unitOfWork.CommitAsync(ct);
        persistence.RecordCommercialActivity(rights, "stock_imported", product.Id, null);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return Result<DropStockImportReceipt>.Success(receipt);
    }
}
