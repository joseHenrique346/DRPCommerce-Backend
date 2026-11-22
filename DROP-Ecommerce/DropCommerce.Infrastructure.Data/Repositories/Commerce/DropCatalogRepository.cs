using Microsoft.EntityFrameworkCore;
using DropCommerce.Domain.Entity;
using DropCommerce.Application.Contracts;
using DropCommerce.Infrastructure.Data.Context;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropCatalogRepository(DropCommerceDbContext db) : DropCommercePersistence(db), IDropCatalogRepository
{
    public Task<List<DropCatalogProduct>> ListProductsAsync(long? afterId, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().Include(Product => Product.Variants).AsNoTracking()
            .Where(Product => !afterId.HasValue || Product.Id > afterId.Value)
            .OrderBy(Product => Product.Id).Take(50).ToListAsync(ct);
    }

    public Task<DropCatalogProduct?> FindProductWithVariantsAsync(long productId, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().Include(Product => Product.Variants).SingleOrDefaultAsync(Product => Product.Id == productId, ct);
    }

    public Task<DropEvent?> FindEventAsync(long eventId, CancellationToken ct)
    {
        return Context.DropEvents.SingleOrDefaultAsync(Event => Event.Id == eventId, ct);
    }

    public Task<DropProduct?> FindParticipationAsync(long eventId, long productId, CancellationToken ct)
    {
        return Context.DropProducts.Where(Product => Product.DropEventId == eventId && Product.CatalogProductId == productId)
            .OrderBy(Product => Product.Id).FirstOrDefaultAsync(ct);
    }

    public Task<List<DropCatalogProduct>> ListCatalogAsync(CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().AsNoTracking().ToListAsync(ct);
    }

    public Task<List<DropProduct>> ListParticipationsAsync(CancellationToken ct)
    {
        return Context.DropProducts.Include(Product => Product.DropEvent).AsNoTracking().ToListAsync(ct);
    }

    public Task<List<DropProduct>> ListActiveParticipationsAsync(CancellationToken ct)
    {
        return Context.DropProducts.Include(Product => Product.DropEvent).Include(Product => Product.CatalogProduct)
            .Where(Product => Product.IsActive).OrderBy(Product => Product.CreatedAt).ThenBy(Product => Product.Id).ToListAsync(ct);
    }

    public Task<DropCatalogProduct?> FindProductAsync(long productId, CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().SingleOrDefaultAsync(Product => Product.Id == productId, ct);
    }

    public Task<int> CountAvailableCatalogAsync(CancellationToken ct)
    {
        return Context.Set<DropCatalogProduct>().CountAsync(Product => !Product.IsContractBlocked, ct);
    }

    public Task<List<DropProduct>> ListProductParticipationsAsync(long productId, CancellationToken ct)
    {
        return Context.DropProducts.Where(Product => Product.CatalogProductId == productId).ToListAsync(ct);
    }

    public Task<bool> HasReservationsAsync(List<long> ids, CancellationToken ct)
    {
        return Context.DropReservations.IgnoreQueryFilters().AnyAsync(Reservation => ids.Contains(Reservation.DropProductId) && Reservation.DropReservationStatusId == 1, ct);
    }

    public Task<bool> HasPendingOrdersAsync(List<long> ids, CancellationToken ct)
    {
        return Context.DropOrderItems.IgnoreQueryFilters().AnyAsync(Item => ids.Contains(Item.DropProductId) &&
                (Item.DropOrder.DropOrderStatusId >= 1 && Item.DropOrder.DropOrderStatusId <= 4 ||
                    Item.DropOrder.PaymentOperationId != null && (Item.DropOrder.FinancialReviewCode != null ||
                        Item.DropOrder.DropOrderPaymentStatusId == 1 && Item.DropOrder.PaymentResolution != "not_submitted")), ct);
    }

    public Task<List<DropCommercialActivity>> ListActivityAsync(long? beforeId, CancellationToken ct)
    {
        return Context.Set<DropCommercialActivity>().AsNoTracking()
            .Where(Activity => !beforeId.HasValue || Activity.Id < beforeId.Value)
            .OrderByDescending(Activity => Activity.Id).Take(100).ToListAsync(ct);
    }
}
