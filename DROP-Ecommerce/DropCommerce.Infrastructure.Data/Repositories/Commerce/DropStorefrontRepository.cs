using DropCommerce.Application.Contracts;
using DropCommerce.Domain.Entity;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Repositories;

public sealed class DropStorefrontRepository(DropCommerceDbContext db) : DropCommercePersistence(db), IDropStorefrontRepository
{
    public Task<DropStorefrontEventResponse?> FindPublicEventAsync(long id, CancellationToken ct)
    {
        return Context.DropEvents.AsNoTracking().Where(Event => Event.Id == id && Event.IsPublic &&
            Event.DropEndsAt > DateTime.UtcNow && Event.DropEventStatusId != 1 && Event.DropEventStatusId != 7 && Event.DropEventStatusId != 8)
            .Select(Event => new DropStorefrontEventResponse(
                Event.Id, Event.Name, Event.Slug, Event.Description, Event.CoverImageUrl, Event.BannerImageUrl,
                Event.RequiresRegistration, Event.RegistrationStartsAt, Event.RegistrationEndsAt, Event.QueueOpensAt,
                Event.DropStartsAt, Event.DropEndsAt, 15,
                Event.ListDropProduct.Where(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked &&
                    Product.CatalogProduct != null && !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked &&
                    !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
                    (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null))
                    .Select(Product => new DropStorefrontProductResponse(
                        Product.Id, Product.CatalogProduct!.Name, Product.CatalogProduct.Description,
                        Product.MaxPerCustomer,
                        Product.MaxPerCustomer >= Product.CatalogProduct.Variants.Sum(Variant => Variant.QuantityOnHand),
                        Product.CatalogProduct.Variants.Select(Variant => new DropStorefrontVariantResponse(
                            Variant.Id, Variant.SKU, Variant.OptionsJson, Variant.Price,
                            Variant.QuantityOnHand - Variant.QuantityReserved))))))
            .SingleOrDefaultAsync(ct);
    }

    public Task<List<DropStorefrontEventSummary>> ListPublicEventsAsync(CancellationToken ct)
    {
        return Context.DropEvents.AsNoTracking()
            .Where(Event => Event.IsPublic && Event.DropEndsAt > DateTime.UtcNow && Event.DropEventStatusId != 1 && Event.DropEventStatusId != 7 && Event.DropEventStatusId != 8 &&
                Event.ListDropProduct.Any(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked &&
                    Product.CatalogProduct != null && !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked &&
                    !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
                    (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null)))
            .OrderBy(Event => Event.DropStartsAt).Take(100)
            .Select(Event => new DropStorefrontEventSummary(
                Event.Id, Event.Name, Event.Slug, Event.Description, Event.Price, Event.CoverImageUrl,
                Event.DropStartsAt, Event.DropEndsAt, Event.RequiresRegistration)).ToListAsync(ct);
    }

    public Task LockEventAsync(long id, long tenant, CancellationToken ct)
    {
        return Context.Database.ExecuteSqlInterpolatedAsync(
            $"SELECT id FROM evento WHERE id = {id} AND enterprise_id = {tenant} FOR UPDATE", ct);
    }

    public Task<DropEvent?> FindEventAsync(long id, CancellationToken ct)
    {
        return Context.DropEvents.SingleOrDefaultAsync(Event => Event.Id == id, ct);
    }

    public Task<bool> HasEligibleProductAsync(long id, CancellationToken ct)
    {
        return Context.DropProducts.AnyAsync(Product => Product.DropEventId == id && !Product.IsContractBlocked && Product.IsActive &&
            Product.CatalogProduct != null && !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked &&
            !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
            (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null), ct);
    }

    public Task<DropRegistration?> FindRegistrationAsync(long id, long customerId, CancellationToken ct)
    {
        return Context.DropRegistrations.SingleOrDefaultAsync(Registration => Registration.DropEventId == id && Registration.CustomerId == customerId, ct);
    }

    public Task<List<DropCustomerRegistrationResponse>> ListRegistrationsAsync(CancellationToken ct)
    {
        return Context.DropRegistrations.AsNoTracking().Select(Registration => new DropCustomerRegistrationResponse(
            Registration.Id, Registration.DropEventId, Registration.DropRegistrationStatusId,
            Registration.IsEligible, Registration.RegisteredAt)).ToListAsync(ct);
    }
}
