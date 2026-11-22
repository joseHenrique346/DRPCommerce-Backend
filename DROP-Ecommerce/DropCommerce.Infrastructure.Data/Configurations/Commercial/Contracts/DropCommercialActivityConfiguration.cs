using DropCommerce.Domain.Entity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace DropCommerce.Infrastructure.Data.Configurations;

public sealed class DropCommercialActivityConfiguration : IEntityTypeConfiguration<DropCommercialActivity>
{
    public void Configure(EntityTypeBuilder<DropCommercialActivity> builder)
    {
        builder.ToTable("atividade_comercial");
        builder.HasKey(Activity => Activity.Id);
        builder.Property(Activity => Activity.Action).HasMaxLength(80);
        builder.HasIndex(Activity => new { Activity.EnterpriseId, Activity.Id });
    }
}
