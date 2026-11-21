using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Contracts.Drop;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Contracts;

public sealed record StartDropStockTransfer(string IdempotencyKey, long ExpectedContractRevision);

public sealed class DropStockTransferService(StoreCommerceDbContext db, ICurrentIdentity actor,
    IIdentityOperationScope identity, DropStockTransferScope transfers)
{
    public async Task<DropStockExport> StartAsync(long productId, StartDropStockTransfer request, CancellationToken ct)
    {
        actor.RequirePermission("products.edit");
        var tenant = actor.EnterpriseId;

        if (string.IsNullOrWhiteSpace(request.IdempotencyKey) || request.IdempotencyKey.Trim().Length > 100)
            throw Failure("DropStock.InvalidKey", "Informe uma chave de transferência válida.");

        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, productId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, productId, ct);
        var key = request.IdempotencyKey.Trim();
        var previous = await db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Transfer => Transfer.EnterpriseId == tenant && Transfer.IdempotencyKey == key, ct);

        if (previous is not null)
        {
            if (previous.ProductId != productId)
                throw Failure("DropStock.IdempotencyConflict", "A chave foi usada em outro produto.");

            return JsonSerializer.Deserialize<DropStockExport>(previous.SnapshotJson)!;
        }

        var enterprise = await db.Enterprises.IgnoreQueryFilters().AsNoTracking().SingleAsync(Enterprise => Enterprise.Id == tenant, ct);
        var rights = EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);

        if (!rights.AllowsDrop || request.ExpectedContractRevision != enterprise.ContractRevision)
            throw Failure("DropStock.ContractChanged", "Atualize o plano e a revisão antes de transferir.");

        var product = await db.Products.IgnoreQueryFilters().SingleOrDefaultAsync(Product => Product.Id == productId &&
            Product.EnterpriseId == tenant && !Product.IsDeleted, ct)
            ?? throw new SecurityFailure(404, "DropStock.ProductNotFound", "Produto não encontrado.");

        if (product.IsActive || product.IsTransferredToDrop)
            throw Failure("DropStock.ArchiveFirst", "Arquive o produto e conclua a transferência anterior antes de transferir.");

        if (product.Name.Length > 255 || product.Description.Length > 10000)
            throw Failure("DropStock.InvalidCatalog", "Ajuste os dados do produto aos limites do catálogo DROP antes de transferir.");

        var variants = await db.ProductVariants.IgnoreQueryFilters().Where(Variant => Variant.ProductId == productId &&
            Variant.EnterpriseId == tenant && !Variant.IsDeleted).OrderBy(Variant => Variant.Id).ToListAsync(ct);

        if (variants.Count is < 1 or > 200 || variants.Any(Variant => Variant.ReservedQuantity != 0 || Variant.StockQuantity is < 0 or > 1000000 ||
            Variant.AttributesJson.Length > 4000 || (Variant.Price ?? product.Price) <= 0 || (Variant.Price ?? product.Price) > 9999999999.99m))
            throw Failure("DropStock.StockInUse", "Há reservas ou saldo incompatível com a transferência.");

        var reserved = await db.StockReservationLines.IgnoreQueryFilters().AnyAsync(Line => Line.EnterpriseId == tenant &&
            Line.ProductVariant.ProductId == productId && Line.StockReservation.Status == StockReservationStatus.Reserved, ct);
        var openOrders = await db.OrderItems.IgnoreQueryFilters().AnyAsync(Item => Item.Order.EnterpriseId == tenant &&
            Item.ProductId == productId && Item.Order.OrderStatusId >= 1 && Item.Order.OrderStatusId <= 4, ct);
        var uncertain = await db.PaymentAttempts.IgnoreQueryFilters().AnyAsync(Attempt => Attempt.EnterpriseId == tenant &&
            Attempt.Order.ListOrderItem.Any(Item => Item.ProductId == productId) &&
            (Attempt.Status == PaymentCreationStatus.Processing || Attempt.Status == PaymentCreationStatus.OutcomeUnknown ||
                Attempt.Order.OrderStatusId == 1 && Attempt.Status == PaymentCreationStatus.Requested), ct);
        var operations = await db.PaymentOperations.IgnoreQueryFilters().AnyAsync(Operation => Operation.EnterpriseId == tenant &&
            Operation.Order.ListOrderItem.Any(Item => Item.ProductId == productId) &&
            Operation.Status != PaymentOperationStatus.Succeeded && Operation.Status != PaymentOperationStatus.Superseded, ct);
        var pendingPayments = await db.Transactions.IgnoreQueryFilters().AnyAsync(Transaction => Transaction.Order.EnterpriseId == tenant &&
            Transaction.Order.ListOrderItem.Any(Item => Item.ProductId == productId) && Transaction.TransactionTypeId == 1 &&
            (Transaction.TransactionStatusId == 1 || Transaction.TransactionStatusId == 2), ct);

        if (reserved || openOrders || uncertain || operations || pendingPayments)
            throw Failure("DropStock.PendingOperations", "Resolva reservas, pedidos e pagamentos pendentes antes de transferir.");

        var export = new DropStockExport(Guid.NewGuid(), tenant, productId, product.Name, product.Description,
            variants.Select(Variant => new DropStockVariant(Variant.Id, Variant.SKU, Variant.AttributesJson,
                Variant.Price ?? product.Price, Variant.StockQuantity)).ToList(), product.CreatedAt);
        var row = DropStockTransfer.Withdraw(tenant, productId, export.OperationId, key, JsonSerializer.Serialize(export));
        db.Add(row);
        product.TransferStockControl(true);

        foreach (var variant in variants.Where(Variant => Variant.StockQuantity != 0))
        {
            var quantity = variant.StockQuantity;
            variant.AdjustStock(-quantity, false);
            db.Add(StockMovement.Create(variant, null, StockMovementType.Adjustment,
                "drop-export:" + export.OperationId.ToString("N") + ":" + variant.Id,
                -quantity, 0, "Transferência do controle de estoque ao DROP", actor.UserId, DateTime.UtcNow));
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return export;
    }

    public async Task AcknowledgeAsync(long tenant, Guid operationId, long catalogId, CancellationToken ct)
    {
        if (catalogId < 1)
            throw Failure("DropStock.InvalidDestination", "Destino inválido.");

        var source = await db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(Transfer => Transfer.OperationId == operationId && Transfer.EnterpriseId == tenant, ct);
        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, source.ProductId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, source.ProductId, ct);
        var row = await db.Set<DropStockTransfer>().IgnoreQueryFilters().SingleAsync(Transfer => Transfer.Id == source.Id, ct);
        row.Imported(catalogId, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    public async Task ReturnAsync(DropStockReturn request, CancellationToken ct)
    {
        var tenant = request.EnterpriseId;
        var source = await db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Transfer =>
            Transfer.OperationId == request.OriginalExportId && Transfer.EnterpriseId == tenant && Transfer.ProductId == request.ProductId, ct)
            ?? throw new SecurityFailure(404, "DropStock.ExportNotFound", "Transferência de origem não encontrada.");
        using var operation = identity.Begin();
        using var transfer = transfers.Begin(tenant, source.ProductId);
        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await LockAsync(tenant, source.ProductId, ct);
        var row = await db.Set<DropStockTransfer>().IgnoreQueryFilters().SingleAsync(Transfer => Transfer.Id == source.Id, ct);

        if (row.ReturnOperationId.HasValue)
        {
            if (row.ReturnOperationId != request.OperationId)
                throw Failure("DropStock.ReturnConflict", "Outra operação já concluiu esse retorno.");

            return;
        }

        var product = await db.Products.IgnoreQueryFilters().SingleAsync(Product => Product.Id == source.ProductId && Product.EnterpriseId == tenant, ct);
        var original = JsonSerializer.Deserialize<DropStockExport>(source.SnapshotJson)!;

        if (!product.IsTransferredToDrop || product.IsActive || product.IsDeleted || request.OperationId == Guid.Empty ||
            request.Variants is null || request.Variants.Count != original.Variants.Count ||
            request.Variants.Select(Variant => Variant.SourceVariantId).Distinct().Count() != request.Variants.Count ||
            !request.Variants.Select(Variant => Variant.SourceVariantId).Order().SequenceEqual(original.Variants.Select(Variant => Variant.SourceVariantId).Order()))
            throw Failure("DropStock.InvalidReturn", "O retorno não corresponde ao estoque transferido.");

        foreach (var returned in request.Variants)
        {
            var variant = await db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.Id == returned.SourceVariantId &&
                Variant.ProductId == product.Id && Variant.EnterpriseId == tenant && !Variant.IsDeleted, ct);

            var exported = original.Variants.Single(Variant => Variant.SourceVariantId == returned.SourceVariantId);

            if (variant.StockQuantity != 0 || variant.ReservedQuantity != 0 || returned.Quantity < 0 || returned.Quantity > exported.Quantity ||
                request.OperationId == request.OriginalExportId && (row.ImportedAt.HasValue || returned.Quantity != exported.Quantity))
                throw Failure("DropStock.ReturnInUse", "O SKU de origem não está disponível para receber o retorno.");

            if (returned.Quantity == 0)
                continue;

            variant.AdjustStock(returned.Quantity, false);
            db.Add(StockMovement.Create(variant, null, StockMovementType.Return,
                "drop-return:" + request.OperationId.ToString("N") + ":" + variant.Id,
                returned.Quantity, 0, "Retorno do controle de estoque do DROP", null, DateTime.UtcNow));
        }

        product.TransferStockControl(false);
        row.Returned(request.OperationId, DateTime.UtcNow);
        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    private async Task LockAsync(long tenant, long productId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM produto WHERE id = {productId} AND enterprise_id = {tenant} FOR UPDATE", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM variante_produto WHERE product_id = {productId} AND enterprise_id = {tenant} ORDER BY id FOR UPDATE", ct);
    }

    private static SecurityFailure Failure(string code, string message)
    {
        return new SecurityFailure(409, code, message);
    }
}
