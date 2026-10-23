using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace SharedCommerce.Infrastructure.Data.Context;

public sealed class AppearanceWriteScope
{
    private long? tenant;

    public static bool IsAppearanceEntity(object entity)
    {
        return entity is AppearanceEntity or AppearanceRevision or AppearanceEditSession or ThemeRelease or BaseStaticEntity;
    }

    public IDisposable Begin(long enterpriseId)
    {
        if (tenant.HasValue)
            throw new InvalidOperationException("Uma operação de aparência já está aberta.");

        tenant = enterpriseId;

        return new Lease(this);
    }

    public void Guard(DbContext db)
    {
        var entries = db.ChangeTracker.Entries()
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var entry in entries)
        {
            if (entry.Entity is BaseStaticEntity)
                throw new AppearanceFailure(403, "Appearance.StaticClassification", "As classificações de aparência são mantidas por migrations.");

            long? owner = entry.Entity switch
            {
                AppearanceEntity appearance => appearance.EnterpriseId,
                AppearanceRevision revision => revision.EnterpriseId,
                AppearanceEditSession session => session.EnterpriseId,
                ThemeRelease => -1,
                _ => null
            };

            if (!owner.HasValue)
                continue;

            if (tenant != owner || entry.State == EntityState.Deleted)
                throw new AppearanceFailure(403, "Appearance.UseWorkflow", "Use os fluxos próprios de aparência.");

            if (entry.Entity is AppearanceRevision && entry.State != EntityState.Added)
                throw new AppearanceFailure(409, "Appearance.ImmutableRevision", "Revisões publicadas são imutáveis.");

            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("EnterpriseId") is not null &&
                entry.Property("EnterpriseId").OriginalValue is long originalTenant && originalTenant != owner)
                throw new AppearanceFailure(403, "Appearance.TenantMismatch", "Não é permitido transferir aparência entre lojas.");

            if (entry.Entity is ThemeRelease && entry.State == EntityState.Modified)
            {
                foreach (var name in new[] { "AppearanceChannelId", "Key", "Version", "RendererContract", "SchemaJson", "DefaultsJson", "Checksum", "Name" })
                {
                    if (entry.Property(name).IsModified)
                        throw new AppearanceFailure(409, "Appearance.ImmutableRelease", "O conteúdo de uma versão de tema é imutável.");
                }
            }
        }
    }

    private sealed class Lease(AppearanceWriteScope scope) : IDisposable
    {
        public void Dispose()
        {
            scope.tenant = null;
        }
    }
}