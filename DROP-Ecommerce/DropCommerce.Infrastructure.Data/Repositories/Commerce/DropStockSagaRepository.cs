using Microsoft.EntityFrameworkCore;
using DropCommerce.Domain.Entity;
using DropCommerce.Application.Contracts;
using DropCommerce.Infrastructure.Data.Context;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropStockSagaRepository(DropCommerceDbContext db) : DropCommercePersistence(db), IDropStockSagaRepository
{
    public Task<DropStockImportReceipt?> FindReceiptAsync(Guid operationId, CancellationToken ct)
    {
        return Context.Set<DropStockImportReceipt>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Receipt => Receipt.OperationId == operationId, ct);
    }

    public Task<DropCatalogProduct?> FindSourceProductAsync(long tenant, long productId, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().IgnoreQueryFilters().Include(Product => Product.Variants)
            .SingleOrDefaultAsync(Product => Product.EnterpriseId == tenant && Product.StoreProductId == productId, ct);
    }

    public Task<int> CountCatalogAsync(long tenant, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().IgnoreQueryFilters().CountAsync(Product =>
            Product.EnterpriseId == tenant && !Product.IsDeleted, ct);
    }

    public Task<DropStockReturnIntent?> FindPendingReturnAsync(long tenant, long productId, CancellationToken ct)
    {
        return Context.Set<DropStockReturnIntent>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Return => Return.EnterpriseId == tenant && Return.CatalogProductId == productId && Return.CompletedAt == null, ct);
    }

    public Task<DropCatalogProduct?> FindProductWithVariantsAsync(long productId, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().Include(Product => Product.Variants)
            .SingleOrDefaultAsync(Product => Product.Id == productId, ct);
    }

    public Task<bool> HasFutureEventsAsync(long productId, long tenant, CancellationToken ct)
    {
        return Context.DropProducts.IgnoreQueryFilters().AnyAsync(Product => Product.CatalogProductId == productId &&
            Product.DropEvent.EnterpriseId == tenant && !Product.IsDeleted && !Product.DropEvent.IsDeleted &&
            Product.DropEvent.DropEndsAt > DateTime.UtcNow && Product.DropEvent.DropEventStatusId != 7 && Product.DropEvent.DropEventStatusId != 8, ct);
    }

    public Task<bool> HasReservationsAsync(long productId, long tenant, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().AnyAsync(Reservation =>
            Reservation.DropProduct.CatalogProductId == productId && Reservation.DropEvent.EnterpriseId == tenant &&
            Reservation.DropReservationStatusId == 1, ct);
    }

    public Task<bool> HasPendingOrdersAsync(long productId, long tenant, CancellationToken ct)
    {
        return Context.DropOrderItems.IgnoreQueryFilters().AnyAsync(Item => Item.DropProduct.CatalogProductId == productId &&
            Item.DropOrder.DropEvent.EnterpriseId == tenant && (Item.DropOrder.DropOrderStatusId >= 1 && Item.DropOrder.DropOrderStatusId <= 4 ||
                Item.DropOrder.PaymentOperationId != null && (Item.DropOrder.FinancialReviewCode != null ||
                    Item.DropOrder.DropOrderPaymentStatusId == 1 && Item.DropOrder.PaymentResolution != "not_submitted")), ct);
    }

    public Task<DropStockReturnIntent> GetReturnAsync(long tenant, Guid operationId, CancellationToken ct)
    {
        return Context.Set<DropStockReturnIntent>().IgnoreQueryFilters().SingleAsync(Return => Return.EnterpriseId == tenant && Return.OperationId == operationId, ct);
    }

    public Task<DropCatalogProduct> GetProductAsync(long productId, long tenant, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().IgnoreQueryFilters().SingleAsync(Product => Product.Id == productId && Product.EnterpriseId == tenant, ct);
    }
}
