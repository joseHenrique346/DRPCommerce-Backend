using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.StaticEntity;
using SharedCommerce.Infrastructure.Data.Configurations;
using Microsoft.EntityFrameworkCore;
using AppearanceEntity = SharedCommerce.Domain.Entity.Appearance;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    public DbSet<AppearanceChannel> AppearanceChannels => Set<AppearanceChannel>();
    public DbSet<AppearanceTargetType> AppearanceTargetTypes => Set<AppearanceTargetType>();
    public DbSet<AppearanceEditSessionStatus> AppearanceEditSessionStatuses => Set<AppearanceEditSessionStatus>();
    public DbSet<AppearanceRevisionOrigin> AppearanceRevisionOrigins => Set<AppearanceRevisionOrigin>();

    private void ConfigureAppearance(ModelBuilder builder)
    {
        builder.ApplyConfiguration(new AppearanceChannelConfiguration());
        builder.ApplyConfiguration(new AppearanceTargetTypeConfiguration());
        builder.ApplyConfiguration(new AppearanceEditSessionStatusConfiguration());
        builder.ApplyConfiguration(new AppearanceRevisionOriginConfiguration());
        builder.ApplyConfiguration(new ThemeReleaseConfiguration());
        builder.ApplyConfiguration(new AppearanceConfiguration());
        builder.ApplyConfiguration(new AppearanceRevisionConfiguration());
        builder.ApplyConfiguration(new AppearanceEditSessionConfiguration());

        builder.Entity<AppearanceEntity>().ToTable(Table => Table.HasCheckConstraint(
            "ck_appearance_store_target", $"appearance_target_type_id = {AppearanceTargetType.Store.Id}"));

        builder.Entity<AppearanceEntity>().HasQueryFilter(Appearance => Appearance.EnterpriseId == _tenantProvider.GetEnterpriseId());
        builder.Entity<AppearanceEditSession>().HasQueryFilter(AppearanceEditSession => AppearanceEditSession.EnterpriseId == _tenantProvider.GetEnterpriseId());
        builder.Entity<AppearanceRevision>().HasQueryFilter(AppearanceRevision => AppearanceRevision.EnterpriseId == _tenantProvider.GetEnterpriseId());

        builder.Entity<AppearanceEntity>().HasOne<StoreCommerce.Domain.Entity.Enterprise>().WithMany()
            .HasForeignKey(Appearance => Appearance.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
    }
}
