using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public sealed class PlatformSubscriptionConfiguration : IEntityTypeConfiguration<PlatformSubscription>
{
    public void Configure(EntityTypeBuilder<PlatformSubscription> builder)
    {
        builder.ToTable("assinatura_plataforma");
        builder.HasKey(Subscription => Subscription.Id);
        builder.Property(Subscription => Subscription.Reference).HasMaxLength(41);
        builder.Property(Subscription => Subscription.AccountId).HasMaxLength(40);
        builder.Property(Subscription => Subscription.RemoteId).HasMaxLength(200);
        builder.Property(Subscription => Subscription.CheckoutUrl).HasMaxLength(2000);
        builder.Property(Subscription => Subscription.RemoteStatus).HasMaxLength(40);
        builder.Property(Subscription => Subscription.ReviewCode).HasMaxLength(100);
        builder.Property(Subscription => Subscription.RetainedMembershipIdsJson).HasColumnType("jsonb");
        builder.HasIndex(Subscription => Subscription.Reference).IsUnique();
        builder.HasIndex(Subscription => new { Subscription.AccountId, Subscription.Environment, Subscription.RemoteId }).IsUnique();
        builder.HasIndex(Subscription => Subscription.EnterpriseId).IsUnique().HasFilter("closed_at IS NULL");
        builder.HasOne<Enterprise>().WithMany().HasForeignKey(Subscription => Subscription.EnterpriseId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseContractChange>().WithMany().HasForeignKey(Subscription => Subscription.OfferId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne<EnterpriseContractChange>().WithMany().HasForeignKey(Subscription => Subscription.PendingOfferId).OnDelete(DeleteBehavior.Restrict);
    }
}
