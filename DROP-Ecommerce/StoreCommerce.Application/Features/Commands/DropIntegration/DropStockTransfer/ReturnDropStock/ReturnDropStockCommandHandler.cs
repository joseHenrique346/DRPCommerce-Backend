using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Inventory;
using StoreCommerce.Domain.Enums.Inventory;
using System.Text.Json;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class ReturnDropStockCommandHandler(
    IDropStockTransferRepository persistence,
    IIdentityOperationScope identity,
    DropStockTransferScope transfers,
    IdentityHandlerContext context)
    : IdentityHandlerBase<ReturnDropStockCommand, bool>(context)
{
    protected override async Task<bool> ExecuteAsync(ReturnDropStockCommand command, CancellationToken ct)
    {
        DropStockReturn request = command.Input;

        var tenant = request.EnterpriseId;
        var source = await persistence.FindOriginalExportAsync(request.OriginalExportId, tenant, request.ProductId, ct)
            ?? throw new SecurityFailure(404, "DropStock.ExportNotFound", "Transferência de origem não encontrada.");
        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, source.ProductId);
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, source.ProductId, ct);
        var row = await persistence.GetTransferAsync(source.Id, ct);

        if (row.ReturnOperationId.HasValue)
        {
            if (row.ReturnOperationId != request.OperationId)
                throw Failure("DropStock.ReturnConflict", "Outra operação já concluiu esse retorno.");

            return true;
        }

        var product = await persistence.GetProductAsync(source.ProductId, tenant, ct);
        var original = JsonSerializer.Deserialize<DropStockExport>(source.SnapshotJson)!;

        if (!product.IsTransferredToDrop || product.IsActive || product.IsDeleted || request.OperationId == Guid.Empty ||
            request.Variants is null || request.Variants.Count != original.Variants.Count ||
            request.Variants.Select(Variant => Variant.SourceVariantId).Distinct().Count() != request.Variants.Count ||
            !request.Variants.Select(Variant => Variant.SourceVariantId).Order().SequenceEqual(original.Variants.Select(Variant => Variant.SourceVariantId).Order()))
            throw Failure("DropStock.InvalidReturn", "O retorno não corresponde ao estoque transferido.");

        foreach (var returned in request.Variants)
        {
            var variant = await persistence.GetVariantAsync(returned.SourceVariantId, product.Id, tenant, ct);

            var exported = original.Variants.Single(Variant => Variant.SourceVariantId == returned.SourceVariantId);

            if (variant.StockQuantity != 0 || variant.ReservedQuantity != 0 || returned.Quantity < 0 || returned.Quantity > exported.Quantity ||
                request.OperationId == request.OriginalExportId && (row.ImportedAt.HasValue || returned.Quantity != exported.Quantity))
                throw Failure("DropStock.ReturnInUse", "O SKU de origem não está disponível para receber o retorno.");

            if (returned.Quantity == 0)
                continue;

            variant.AdjustStock(returned.Quantity, false);
            persistence.Add(StockMovement.Create(variant, null, StockMovementType.Return,
                "drop-return:" + request.OperationId.ToString("N") + ":" + variant.Id,
                returned.Quantity, 0, "Retorno do controle de estoque do DROP", null, DateTime.UtcNow));
        }

        product.TransferStockControl(false);
        row.Returned(request.OperationId, DateTime.UtcNow);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return true;
    }

    private Task LockAsync(long tenant, long productId, CancellationToken ct)
    {
        return persistence.LockStockAsync(tenant, productId, ct);
    }

    private static SecurityFailure Failure(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }
}
