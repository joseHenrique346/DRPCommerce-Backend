using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using Microsoft.EntityFrameworkCore;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    public DbSet<UserRefreshToken> UserRefreshTokens => Set<UserRefreshToken>();
    public DbSet<CustomerRefreshToken> CustomerRefreshTokens => Set<CustomerRefreshToken>();
    public DbSet<UserActionToken> UserActionTokens => Set<UserActionToken>();
    public DbSet<CustomerActionToken> CustomerActionTokens => Set<CustomerActionToken>();
    public DbSet<EnterpriseInvitation> EnterpriseInvitations => Set<EnterpriseInvitation>();
    public DbSet<InvitationAccessRole> InvitationAccessRoles => Set<InvitationAccessRole>();
    public DbSet<OwnershipTransfer> OwnershipTransfers => Set<OwnershipTransfer>();

    private void ConfigureSecurityFilters(ModelBuilder b)
    {
        b.Entity<CustomerRefreshToken>().HasQueryFilter(x => x.EnterpriseId == _tenantProvider.GetEnterpriseId());
        b.Entity<CustomerActionToken>().HasQueryFilter(x => x.EnterpriseId == _tenantProvider.GetEnterpriseId());
        b.Entity<EnterpriseInvitation>().HasQueryFilter(x => x.EnterpriseId == _tenantProvider.GetEnterpriseId());
        b.Entity<InvitationAccessRole>().HasQueryFilter(x => x.EnterpriseId == _tenantProvider.GetEnterpriseId());
        b.Entity<OwnershipTransfer>().HasQueryFilter(x => x.EnterpriseId == _tenantProvider.GetEnterpriseId());
        b.Entity<EnterpriseDomain>().Property(x => x.VerificationTokenHash).HasMaxLength(64);
        b.Entity<EnterpriseMembership>().Property(x => x.AccessVersion).IsConcurrencyToken();
        b.Entity<Enterprise>().HasQueryFilter(x => _identityScope.IsOpen || x.Id == _tenantProvider.GetEnterpriseId());
    }

    private async Task GuardWritesAsync(CancellationToken ct)
    {
        if (_identityScope.IsOpen)
            return;

        var changes = ChangeTracker.Entries()
            .Where(Entry => !SharedCommerce.Infrastructure.Data.Context.AppearanceWriteScope.IsAppearanceEntity(Entry.Entity))
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted && !Entry.Metadata.IsOwned())
            .ToList();

        if (changes.Count == 0)
            return;

        var tenant = _tenantProvider.GetEnterpriseId();
        var principal = _httpAccessor.HttpContext?.User ?? throw new SecurityFailure(401, "Security.AuthenticationRequired", "Autenticação necessária.");

        if (principal.FindFirst(SecurityNames.Actor)?.Value != "user")
            throw new SecurityFailure(403, "Security.BackofficeRequired", "Use os casos de uso próprios do cliente.");

        foreach (var entry in changes)
        {
            if (entry.Entity is User or Enterprise or EnterpriseMembership or EnterpriseDomain or AccessRole or Permission or
                AccessRolePermission or MembershipAccessRole or UserRefreshToken or CustomerRefreshToken or UserActionToken or CustomerActionToken or EnterpriseInvitation or InvitationAccessRole or OwnershipTransfer)
                throw new SecurityFailure(403, "Security.UseIdentityWorkflow", "Use os fluxos específicos de identidade e gestão de acesso.");

            if (entry.Entity is ITenantEntity scoped && scoped.EnterpriseId != tenant)
                throw new SecurityFailure(403, "Security.TenantMismatch", "Registro não pertence à loja autenticada.");

            if (entry.State == EntityState.Modified && entry.Metadata.FindProperty("EnterpriseId") is { } property &&
                entry.Property(property.Name).OriginalValue is long original && original != tenant)
                throw new SecurityFailure(403, "Security.TenantMismatch", "Não é permitido transferir registros entre lojas.");

            if (entry.State == EntityState.Deleted)
                continue;

            switch (entry.Entity)
            {
                case Category category when category.ParentCategoryId.HasValue:
                    await EnsureTenantAsync<Category>(category.ParentCategoryId.Value, tenant, ct);
                    break;
                case Product p:
                    await EnsureTenantAsync<Category>(p.CategoryId, tenant, ct);

                    if (p.SupplierId.HasValue)
                        await EnsureTenantAsync<Supplier>(p.SupplierId.Value, tenant, ct);

                    if (entry.State == EntityState.Modified && !SecurityNames.HasPermission(principal, "products.change_price"))
                    {
                        var originalPrice = await Products.AsNoTracking().Where(x => x.Id == p.Id)
                            .Select(x => new { x.Price, x.CostPrice }).SingleAsync(ct);

                        if (originalPrice.Price != p.Price || originalPrice.CostPrice != p.CostPrice)
                            throw new SecurityFailure(403, "Security.PricePermissionRequired", "Permissão para alterar preço necessária.");
                    }

                    break;
                case Order o:
                    await EnsureTenantAsync<Customer>(o.CustomerId, tenant, ct);

                    if (o.CouponId.HasValue)
                        await EnsureTenantAsync<Coupon>(o.CouponId.Value, tenant, ct);

                    break;
                case Employee e:
                    await EnsureTenantAsync<EnterpriseMembership>(e.EnterpriseMembershipId, tenant, ct);
                    await EnsureTenantAsync<EmployeePosition>(e.PositionId, tenant, ct);
                    await EnsureTenantAsync<Department>(e.DepartmentId, tenant, ct);
                    break;
                case Invoice i:
                    await EnsureTenantAsync<Order>(i.OrderId, tenant, ct);
                    await EnsureTenantAsync<Customer>(i.CustomerId, tenant, ct);

                    if (!await Orders.AnyAsync(x => x.Id == i.OrderId && x.CustomerId == i.CustomerId, ct))
                        throw new SecurityFailure(400, "Security.InvalidRelationship", "Cliente incompatível com o pedido.");

                    break;
                case OrderItem item:
                    await EnsureTenantAsync<Order>(item.OrderId, tenant, ct);

                    if (item.ProductId.HasValue)
                        await EnsureTenantAsync<Product>(item.ProductId.Value, tenant, ct);

                    if (item.ServiceId.HasValue)
                        await EnsureTenantAsync<StoreCommerce.Domain.Entity.Service>(item.ServiceId.Value, tenant, ct);

                    break;
                case Shipment shipment:
                    await EnsureTenantAsync<Order>(shipment.OrderId, tenant, ct);

                    if (shipment.SupplierId.HasValue)
                        await EnsureTenantAsync<Supplier>(shipment.SupplierId.Value, tenant, ct);

                    break;
                case StoreCommerce.Domain.Entity.Transaction transaction:
                    await EnsureTenantAsync<Order>(transaction.OrderId, tenant, ct);
                    await EnsureTenantAsync<Customer>(transaction.CustomerId, tenant, ct);

                    if (!await Orders.AnyAsync(x => x.Id == transaction.OrderId && x.CustomerId == transaction.CustomerId, ct))
                        throw new SecurityFailure(400, "Security.InvalidRelationship", "Cliente incompatível com o pedido.");

                    break;
            }
        }
    }

    private async Task EnsureTenantAsync<T>(long id, long tenant, CancellationToken ct) where T : BaseEntity, ITenantEntity
    {
        if (!await Set<T>().AnyAsync(x => x.Id == id && x.EnterpriseId == tenant, ct))
            throw new SecurityFailure(400, "Security.InvalidRelationship", "Referência inexistente, indisponível ou de outra loja.");
    }
}
