using System.Text.Json;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using MediatR;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed class StartDropStockTransferCommandHandler(
    IDropStockTransferRepository persistence,
    IIdentityOperationScope identity,
    DropStockTransferScope transfers,
    IdentityHandlerContext context)
    : IdentityHandlerBase<StartDropStockTransferCommand, DropStockExport>(context)
{
    protected override async Task<DropStockExport> ExecuteAsync(StartDropStockTransferCommand command, CancellationToken ct)
    {
        long productId = command.ProductId;
        StartDropStockTransfer request = command.Input;

        actor.RequirePermission("products.edit");
        var tenant = actor.EnterpriseId;

        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, productId);
        await using var tx = await unitOfWork.BeginTransactionAsync(ct);
        await LockAsync(tenant, productId, ct);
        var key = request.IdempotencyKey.Trim();
        var previous = await persistence.FindTransferByKeyAsync(tenant, key, ct);

        if (previous is not null)
        {
            if (previous.ProductId != productId)
                throw Failure("DropStock.IdempotencyConflict", "A chave foi usada em outro produto.");

            return JsonSerializer.Deserialize<DropStockExport>(previous.SnapshotJson)!;
        }

        var enterprise = await persistence.GetEnterpriseAsync(tenant, ct);
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        if (!rights.AllowsDrop || request.ExpectedContractRevision != enterprise.ContractRevision)
            throw Failure("DropStock.ContractChanged", "Atualize o plano e a revisão antes de transferir.");

        var product = await persistence.FindProductAsync(productId, tenant, ct)
            ?? throw new SecurityFailure(404, "DropStock.ProductNotFound", "Produto não encontrado.");

        if (product.IsActive || product.IsTransferredToDrop)
            throw Failure("DropStock.ArchiveFirst", "Arquive o produto e conclua a transferência anterior antes de transferir.");

        if (product.Name.Length > 255 || product.Description.Length > 10000)
            throw Failure("DropStock.InvalidCatalog", "Ajuste os dados do produto aos limites do catálogo DROP antes de transferir.");

        var variants = await persistence.ListVariantsAsync(productId, tenant, ct);

        if (variants.Count is < 1 or > 200 || variants.Any(Variant => Variant.ReservedQuantity != 0 || Variant.StockQuantity is < 0 or > 1000000 ||
            Variant.AttributesJson.Length > 4000 || (Variant.Price ?? product.Price) <= 0 || (Variant.Price ?? product.Price) > 9999999999.99m))
            throw Failure("DropStock.StockInUse", "Há reservas ou saldo incompatível com a transferência.");

        var reserved = await persistence.HasReservationsAsync(productId, tenant, ct);
        var openOrders = await persistence.HasOpenOrdersAsync(productId, tenant, ct);
        var uncertain = await persistence.HasUncertainPaymentsAsync(productId, tenant, ct);
        var operations = await persistence.HasPaymentOperationsAsync(productId, tenant, ct);
        var pendingPayments = await persistence.HasPendingPaymentsAsync(productId, tenant, ct);

        if (reserved || openOrders || uncertain || operations || pendingPayments)
            throw Failure("DropStock.PendingOperations", "Resolva reservas, pedidos e pagamentos pendentes antes de transferir.");

        var export = new DropStockExport(Guid.NewGuid(), tenant, productId, product.Name, product.Description,
            variants.Select(Variant => new DropStockVariant(Variant.Id, Variant.SKU, Variant.AttributesJson,
                Variant.Price ?? product.Price, Variant.StockQuantity)).ToList(), product.CreatedAt);
        var row = DropStockTransfer.Withdraw(tenant, productId, export.OperationId, key, JsonSerializer.Serialize(export));
        persistence.Add(row);
        product.TransferStockControl(true);

        foreach (var variant in variants.Where(Variant => Variant.StockQuantity != 0))
        {
            var quantity = variant.StockQuantity;
            variant.AdjustStock(-quantity, false);
            persistence.Add(StockMovement.Create(variant, null, StockMovementType.Adjustment,
                "drop-export:" + export.OperationId.ToString("N") + ":" + variant.Id,
                -quantity, 0, "Transferência do controle de estoque ao DROP", actor.UserId, DateTime.UtcNow));
        }

        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return export;
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
