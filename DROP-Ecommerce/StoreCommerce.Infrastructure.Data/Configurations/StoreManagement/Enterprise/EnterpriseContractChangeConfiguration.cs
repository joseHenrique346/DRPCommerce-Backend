using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class EnterpriseContractChangeConfiguration : IEntityTypeConfiguration<EnterpriseContractChange>
{
    public void Configure(EntityTypeBuilder<EnterpriseContractChange> builder)
    {
        builder.ToTable("solicitacao_contrato_empresa");
        builder.HasKey(Change => Change.Id);
        builder.Property(Change => Change.Id).ValueGeneratedOnAdd();
        builder.Property(Change => Change.CreatedAt).HasDefaultValueSql("CURRENT_TIMESTAMP");
        builder.Property(Change => Change.IdempotencyKey).IsRequired().HasMaxLength(100);
        builder.Property(Change => Change.PlanMonthlyPrice).HasPrecision(12, 2);
        builder.Property(Change => Change.AddOnMonthlyPrice).HasPrecision(12, 2);
        builder.Property(Change => Change.Currency).IsRequired().HasMaxLength(3);
        builder.Property(Change => Change.Status).HasConversion<int>();
        builder.Property(Change => Change.RetainedMembershipIdsJson).HasColumnType("jsonb");
        builder.HasIndex(Change => new { Change.EnterpriseId, Change.IdempotencyKey }).IsUnique();
        builder.HasIndex(Change => Change.EnterpriseId).IsUnique().HasFilter("status = 1")
            .HasDatabaseName("ux_solicitacao_contrato_empresa_pendente");
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Change => Change.EnterpriseId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<User>().WithMany().HasForeignKey(Change => Change.RequestedByUserId)
            .OnDelete(DeleteBehavior.SetNull);
        builder.HasOne<EnterprisePlan>().WithMany().HasForeignKey(Change => Change.PlanId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseAddOn>().WithMany().HasForeignKey(Change => Change.AddOnId)
            .OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterprisePlanAddOn>().WithMany()
            .HasForeignKey(Change => new { Change.PlanId, Change.AddOnId })
            .OnDelete(DeleteBehavior.Restrict);
    }
}
