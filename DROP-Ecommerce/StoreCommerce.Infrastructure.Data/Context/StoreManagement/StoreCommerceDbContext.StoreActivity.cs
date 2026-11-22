using System.Globalization;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private readonly HashSet<StoreActivity> _generatedStoreActivities = [];

    private void AddStoreActivity(long tenant, string type, Product? product, ProductVariant? variant,
        string reason, object details)
    {
        var principal = _httpAccessor.HttpContext?.User;
        long? actor = null;

        if (principal?.FindFirst(SecurityNames.Actor)?.Value == "user" &&
            long.TryParse(principal.FindFirst("sub")?.Value, NumberStyles.None, CultureInfo.InvariantCulture, out var userId))
            actor = userId;

        var correlation = _httpAccessor.HttpContext?.TraceIdentifier ?? "system-" + Guid.NewGuid().ToString("N");
        var activity = StoreActivity.Record(tenant, type, product, variant, actor, reason, correlation, details, DateTime.UtcNow);
        _generatedStoreActivities.Add(activity);
        StoreActivities.Add(activity);
    }

    private void CaptureProductActivities()
    {
        foreach (var entry in ChangeTracker.Entries<Product>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified).ToList())
        {
            var product = entry.Entity;
            string type;

            if (entry.State == EntityState.Added)
                type = "product_created";
            else if (!entry.Property(Product => Product.IsDeleted).OriginalValue && product.IsDeleted)
                type = "product_deleted";
            else if (entry.Property(Product => Product.IsDeleted).OriginalValue && !product.IsDeleted)
                type = "product_restored";
            else if (product.IsContractBlocked && (entry.Property(Product => Product.IsContractBlocked).IsModified ||
                entry.Property(Product => Product.ContractBlockReason).IsModified))
                type = "product_contract_blocked";
            else if (entry.Property(Product => Product.IsContractBlocked).OriginalValue && !product.IsContractBlocked)
                type = product.IsActive ? "product_reactivated" : "product_unblocked";
            else if (entry.Property(Product => Product.IsActive).IsModified)
                type = product.IsActive ? "product_published" : "product_archived";
            else
                type = "product_updated";

            var fields = entry.Properties.Where(Property => Property.IsModified &&
                Property.Metadata.Name != nameof(Product.UpdatedAt)).Select(Property => Property.Metadata.Name).ToArray();
            AddStoreActivity(product.EnterpriseId, type, product, null,
                product.ContractBlockReason ?? "catalog_operation", new { fields });
        }

        foreach (var entry in ChangeTracker.Entries<ProductVariant>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified).ToList())
        {
            if (entry.Entity.IsDefault && entry.State == EntityState.Added)
                continue;

            if (entry.State == EntityState.Modified && IsTechnicalStockChange(entry))
                continue;

            var variant = entry.Entity;
            var type = entry.State == EntityState.Added ? "variant_created" :
                variant.IsDeleted ? "variant_deleted" : "variant_updated";
            var fields = entry.Properties.Where(Property => Property.IsModified &&
                Property.Metadata.Name != nameof(ProductVariant.UpdatedAt)).Select(Property => Property.Metadata.Name).ToArray();
            AddStoreActivity(variant.EnterpriseId, type, variant.Product, variant, "catalog_operation", new { fields });
        }
    }

    private void GuardStoreActivityWrites()
    {
        foreach (var entry in ChangeTracker.Entries<StoreActivity>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.State != EntityState.Added || !_generatedStoreActivities.Contains(entry.Entity))
                throw new SecurityFailure(403, "StoreActivity.Immutable",
                    "O histórico é imutável e só pode ser acrescentado pelos fluxos do servidor.");
        }
    }
}
