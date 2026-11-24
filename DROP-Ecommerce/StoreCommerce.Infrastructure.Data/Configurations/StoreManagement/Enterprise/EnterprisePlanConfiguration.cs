using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Configurations;

public class EnterprisePlanConfiguration : IEntityTypeConfiguration<EnterprisePlan>
{
    public void Configure(EntityTypeBuilder<EnterprisePlan> builder)
    {
        builder.ToTable("plano_empresa");
        builder.HasKey(Plan => Plan.Id);
        builder.Property(Plan => Plan.Id).ValueGeneratedNever();
        builder.Property(Plan => Plan.Description).IsRequired().HasMaxLength(100);
        builder.Property(Plan => Plan.Summary).IsRequired().HasMaxLength(300);
        builder.Property(Plan => Plan.AllowsVitrine).IsRequired();
        builder.Property(Plan => Plan.AllowsEmployees).IsRequired();
        builder.Property(Plan => Plan.AllowsCustomDomain).IsRequired();
        builder.Property(Plan => Plan.AllowsCustomTheme).IsRequired();
        builder.Property(Plan => Plan.IncludesDropModule).IsRequired();
        builder.Property(Plan => Plan.Code).IsRequired().HasMaxLength(40);
        builder.HasIndex(Plan => Plan.Code).IsUnique();
        builder.Property(Plan => Plan.MonthlyPrice).HasPrecision(12, 2);
        builder.Property(Plan => Plan.StoreProductLimit).IsRequired();
        builder.Property(Plan => Plan.DropProductLimit).IsRequired();
        builder.Property(Plan => Plan.EmployeeLimit).IsRequired();
        builder.Property(Plan => Plan.StoreThemeLimit).IsRequired();
        builder.Property(Plan => Plan.DropThemeLimit).IsRequired();
        builder.Property(Plan => Plan.AllowsLogoCustomization).IsRequired();
        builder.Property(Plan => Plan.AllowsFullCustomization).IsRequired();
        builder.HasData(EnterprisePlan.GetAll<EnterprisePlan>()
            .Select(Plan => new
            {
                Plan.Id, Plan.Description, Plan.Summary, Plan.Code, Plan.MonthlyPrice,
                Plan.StoreProductLimit, Plan.DropProductLimit, Plan.EmployeeLimit, Plan.StoreThemeLimit, Plan.DropThemeLimit,
                Plan.AllowsVitrine, Plan.AllowsEmployees, Plan.AllowsCustomDomain, Plan.AllowsCustomTheme,
                Plan.IncludesDropModule, Plan.AllowsLogoCustomization, Plan.AllowsFullCustomization
            }));
    }
}
