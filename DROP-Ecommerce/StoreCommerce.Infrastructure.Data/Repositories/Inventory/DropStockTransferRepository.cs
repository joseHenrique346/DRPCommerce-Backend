using StoreCommerce.Application.Interfaces.Inventory;
using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Enums.Payments;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class DropStockTransferRepository(StoreCommerceDbContext db) : IDropStockTransferRepository
{
    public void Add<TEntity>(TEntity entity) where TEntity : class
    {
        db.Add(entity);
    }

    public void ClearTracking()
    {
        db.ChangeTracker.Clear();
    }

    public Task<DropStockTransfer?> FindTransferByKeyAsync(long tenant, string key, CancellationToken ct)
    {
        return db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Transfer => Transfer.EnterpriseId == tenant && Transfer.IdempotencyKey == key, ct);
    }

    public Task<Enterprise> GetEnterpriseAsync(long tenant, CancellationToken ct)
    {
        return db.Enterprises.IgnoreQueryFilters().AsNoTracking().SingleAsync(Enterprise => Enterprise.Id == tenant, ct);
    }

    public Task<Product?> FindProductAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.Products.IgnoreQueryFilters().SingleOrDefaultAsync(Product => Product.Id == productId &&
            Product.EnterpriseId == tenant && !Product.IsDeleted, ct);
    }

    public Task<List<ProductVariant>> ListVariantsAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.ProductVariants.IgnoreQueryFilters().Where(Variant => Variant.ProductId == productId &&
            Variant.EnterpriseId == tenant && !Variant.IsDeleted).OrderBy(Variant => Variant.Id).ToListAsync(ct);
    }

    public Task<bool> HasReservationsAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.StockReservationLines.IgnoreQueryFilters().AnyAsync(Line => Line.EnterpriseId == tenant &&
            Line.ProductVariant.ProductId == productId && Line.StockReservation.Status == StockReservationStatus.Reserved, ct);
    }

    public Task<bool> HasOpenOrdersAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.OrderItems.IgnoreQueryFilters().AnyAsync(Item => Item.Order.EnterpriseId == tenant &&
            Item.ProductId == productId && Item.Order.OrderStatusId >= 1 && Item.Order.OrderStatusId <= 4, ct);
    }

    public Task<bool> HasUncertainPaymentsAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.PaymentAttempts.IgnoreQueryFilters().AnyAsync(Attempt => Attempt.EnterpriseId == tenant &&
            Attempt.Order.ListOrderItem.Any(Item => Item.ProductId == productId) &&
            (Attempt.Status == PaymentCreationStatus.Processing || Attempt.Status == PaymentCreationStatus.OutcomeUnknown ||
                Attempt.Order.OrderStatusId == 1 && Attempt.Status == PaymentCreationStatus.Requested), ct);
    }

    public Task<bool> HasPaymentOperationsAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.PaymentOperations.IgnoreQueryFilters().AnyAsync(Operation => Operation.EnterpriseId == tenant &&
            Operation.Order.ListOrderItem.Any(Item => Item.ProductId == productId) &&
            Operation.Status != PaymentOperationStatus.Succeeded && Operation.Status != PaymentOperationStatus.Superseded, ct);
    }

    public Task<bool> HasPendingPaymentsAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.Transactions.IgnoreQueryFilters().AnyAsync(Transaction => Transaction.Order.EnterpriseId == tenant &&
            Transaction.Order.ListOrderItem.Any(Item => Item.ProductId == productId) && Transaction.TransactionTypeId == 1 &&
            (Transaction.TransactionStatusId == 1 || Transaction.TransactionStatusId == 2), ct);
    }

    public Task<DropStockTransfer> ReadTransferAsync(Guid operationId, long tenant, CancellationToken ct)
    {
        return db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .SingleAsync(Transfer => Transfer.OperationId == operationId && Transfer.EnterpriseId == tenant, ct);
    }

    public Task<DropStockTransfer> GetTransferAsync(long transferId, CancellationToken ct)
    {
        return db.Set<DropStockTransfer>().IgnoreQueryFilters().SingleAsync(Transfer => Transfer.Id == transferId, ct);
    }

    public Task<DropStockTransfer?> FindOriginalExportAsync(Guid exportId, long tenant, long productId, CancellationToken ct)
    {
        return db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking().SingleOrDefaultAsync(Transfer =>
            Transfer.OperationId == exportId && Transfer.EnterpriseId == tenant && Transfer.ProductId == productId, ct);
    }

    public Task<Product> GetProductAsync(long productId, long tenant, CancellationToken ct)
    {
        return db.Products.IgnoreQueryFilters().SingleAsync(Product => Product.Id == productId && Product.EnterpriseId == tenant, ct);
    }

    public Task<ProductVariant> GetVariantAsync(long variantId, long productId, long tenant, CancellationToken ct)
    {
        return db.ProductVariants.IgnoreQueryFilters().SingleAsync(Variant => Variant.Id == variantId &&
                Variant.ProductId == productId && Variant.EnterpriseId == tenant && !Variant.IsDeleted, ct);
    }

    public async Task LockStockAsync(long tenant, long productId, CancellationToken ct)
    {
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM empresa WHERE id = {tenant} FOR UPDATE", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM produto WHERE id = {productId} AND enterprise_id = {tenant} FOR UPDATE", ct);
        await db.Database.ExecuteSqlInterpolatedAsync($"SELECT id FROM variante_produto WHERE product_id = {productId} AND enterprise_id = {tenant} ORDER BY id FOR UPDATE", ct);
    }

    public Task<List<string>> PendingSnapshotsAsync(CancellationToken ct)
    {
        return db.Set<DropStockTransfer>().IgnoreQueryFilters().AsNoTracking()
            .Where(Transfer => Transfer.ImportedAt == null && Transfer.ReturnedAt == null)
            .OrderBy(Transfer => Transfer.Id).Take(50).Select(Transfer => Transfer.SnapshotJson).ToListAsync(ct);
    }
}
