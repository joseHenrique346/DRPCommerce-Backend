using SharedCommerce.Application.Security;
using DropCommerce.Infrastructure.Data.Context;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Api.Controllers;

[ApiController]
[Route("api/storefront/drop")]
[Authorize(Policy = SecurityNames.Storefront)]
[EnableRateLimiting("api")]
public sealed class StorefrontDropEventController(DropCommerceDbContext db, DropCommerce.Domain.Interfaces.ITenantProvider tenants) : ControllerBase
{
    [AllowAnonymous]
    [HttpGet("events/{id:long}")]
    public async Task<IActionResult> Event(long id, CancellationToken ct)
    {
        var rights = await db.RefreshCommercialRightsAsync(tenants.GetEnterpriseId(), ct);

        if (!rights.AllowsDrop)
            return NotFound();

        var value = await db.DropEvents.AsNoTracking().Where(Event => Event.Id == id && Event.IsPublic &&
            Event.DropEndsAt > DateTime.UtcNow && Event.DropEventStatusId != 1 && Event.DropEventStatusId != 7 && Event.DropEventStatusId != 8)
            .Select(Event => new
            {
                Event.Id, Event.Name, Event.Slug, Event.Description, Event.CoverImageUrl, Event.BannerImageUrl,
                Event.RequiresRegistration, Event.RegistrationStartsAt, Event.RegistrationEndsAt, Event.QueueOpensAt,
                Event.DropStartsAt, Event.DropEndsAt,
                purchaseWindowMinutes = 15,
                products = Event.ListDropProduct.Where(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked &&
                    Product.CatalogProduct != null && !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked &&
                    !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
                    (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null))
                    .Select(Product => new
                    {
                        participationId = Product.Id, Product.CatalogProduct!.Name, Product.CatalogProduct.Description,
                        Product.MaxPerCustomer,
                        concentrationWarning = Product.MaxPerCustomer >= Product.CatalogProduct.Variants.Sum(Variant => Variant.QuantityOnHand),
                        variants = Product.CatalogProduct.Variants.Select(Variant => new
                        {
                            Variant.Id, Variant.SKU, Variant.OptionsJson, Variant.Price,
                            available = Variant.QuantityOnHand - Variant.QuantityReserved
                        })
                    })
            }).SingleOrDefaultAsync(ct);

        return value is null || !value.products.Any() ? NotFound() : Ok(value);
    }

    /// <summary>
    /// Lista os eventos públicos de drop da loja.
    /// </summary>
    [AllowAnonymous]
    [HttpGet("events")]
    public async Task<IActionResult> Events(CancellationToken ct)
    {
        var rights = await db.RefreshCommercialRightsAsync(tenants.GetEnterpriseId(), ct);

        if (!rights.AllowsDrop)
            return Ok(Array.Empty<object>());

        return Ok(await db.DropEvents
            .AsNoTracking()
            .Where(Event => Event.IsPublic && Event.DropEndsAt > DateTime.UtcNow && Event.DropEventStatusId != 1 && Event.DropEventStatusId != 7 && Event.DropEventStatusId != 8 &&
                Event.ListDropProduct.Any(Product => !Product.IsDeleted && Product.IsActive && !Product.IsContractBlocked &&
                    Product.CatalogProduct != null && !Product.CatalogProduct.IsDeleted && !Product.CatalogProduct.IsContractBlocked &&
                    !Product.CatalogProduct.IsReturning && !Product.CatalogProduct.IsReturned &&
                    (Product.CatalogProduct.StoreProductId == null || Product.CatalogProduct.SourceExportId != null)))
            .OrderBy(x => x.DropStartsAt)
            .Take(100)
            .Select(x => new
            {
                x.Id,
                x.Name,
                x.Slug,
                x.Description,
                x.Price,
                x.CoverImageUrl,
                x.DropStartsAt,
                x.DropEndsAt,
                x.RequiresRegistration
            })
            .ToListAsync(ct));
    }
}

