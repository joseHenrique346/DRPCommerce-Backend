using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Media;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    public DbSet<MediaAsset> MediaAssets => Set<MediaAsset>();

    private async Task GuardMediaWritesAsync(CancellationToken ct)
    {
        var entries = ChangeTracker.Entries<MediaAsset>().Where(Entry =>
            Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted).ToList();

        foreach (var group in entries.GroupBy(Entry => Entry.Entity.EnterpriseId).OrderBy(Group => Group.Key))
        {
            if (_mediaWrites.EnterpriseId != group.Key)
                throw new SecurityFailure(403, "Media.UseWorkflow", "Use os fluxos próprios de mídia.");

            var rights = await ReadAppearanceCapabilitiesAsync(group.Key, "store", ct);

            foreach (var entry in group)
            {
                var retaining = entry.State == EntityState.Modified && entry.Entity.IsRetainedByDrop &&
                    entry.Property(Asset => Asset.IsRetainedByDrop).IsModified &&
                    entry.Properties.All(Property => !Property.IsModified ||
                        Property.Metadata.Name is nameof(MediaAsset.IsRetainedByDrop) or nameof(MediaAsset.UpdatedAt));

                if (entry.State == EntityState.Deleted || entry.State == EntityState.Modified &&
                    !retaining &&
                    (!entry.Entity.IsDeleted || entry.Entity.Content.Length != 0 || entry.Entity.ByteLength != 0 ||
                    entry.Properties.Any(Property => Property.IsModified && Property.Metadata.Name is not
                        (nameof(MediaAsset.IsDeleted) or nameof(MediaAsset.UpdatedAt) or nameof(MediaAsset.Content) or nameof(MediaAsset.ByteLength)))))
                    throw new SecurityFailure(403, "Media.Immutable", "O conteúdo de um logo é imutável.");

                if (entry.State == EntityState.Added)
                {
                    if (!rights.AllowsLogo)
                        throw new SecurityFailure(409, "Media.PlanRequired", "O contrato não permite personalizar o logo.");

                    var png = PngLogoValidator.Validate(entry.Entity.Content);

                    if (png.Hash != entry.Entity.ContentHash || png.Width != entry.Entity.Width || png.Height != entry.Entity.Height ||
                        !png.Content.AsSpan().SequenceEqual(entry.Entity.Content))
                        throw new SecurityFailure(400, "Media.InvalidPng", "O conteúdo de mídia não corresponde ao ativo validado.");
                }

                AddStoreActivity(group.Key, entry.State == EntityState.Added ? "logo_uploaded" :
                    retaining ? "logo_retained_for_drop" : "logo_deleted",
                    null, null, "appearance_media", new { assetId = entry.Entity.Id > 0 ? entry.Entity.Id : (long?)null });
            }
        }
    }
}
