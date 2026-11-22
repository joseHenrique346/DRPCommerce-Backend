using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class StoreActivityConfiguration : IEntityTypeConfiguration<StoreActivity>
{
    public void Configure(EntityTypeBuilder<StoreActivity> builder)
    {
        builder.ToTable("atividade_loja");
        builder.HasKey(Activity => Activity.Id);
        builder.Property(Activity => Activity.Id).ValueGeneratedOnAdd();
        builder.Property(Activity => Activity.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(Activity => Activity.EventType).IsRequired().HasMaxLength(60);
        builder.Property(Activity => Activity.ActorType).IsRequired().HasMaxLength(20);
        builder.Property(Activity => Activity.ReasonCode).IsRequired().HasMaxLength(60);
        builder.Property(Activity => Activity.CorrelationId).IsRequired().HasMaxLength(128);
        builder.Property(Activity => Activity.ProductName).IsRequired().HasMaxLength(Product.NameMaxLength);
        builder.Property(Activity => Activity.SKU).IsRequired().HasMaxLength(Product.SkuMaxLength);
        builder.Property(Activity => Activity.DetailsJson).IsRequired().HasColumnType("jsonb");
        builder.HasIndex(Activity => new { Activity.EnterpriseId, Activity.Id });
        builder.HasIndex(Activity => new { Activity.EnterpriseId, Activity.ProductId, Activity.Id });
        builder.HasIndex(Activity => new { Activity.EnterpriseId, Activity.EventType, Activity.OccurredAt });
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Activity => Activity.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Activity => Activity.Product).WithMany().HasForeignKey(Activity => Activity.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(Activity => Activity.ProductVariant).WithMany().HasForeignKey(Activity => Activity.ProductVariantId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(Activity => Activity.ActorUserId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
