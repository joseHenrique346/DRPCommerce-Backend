using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace DropCommerce.Infrastructure.Data.Context;

public partial class DropCommerceDbContext
{
    private long? CurrentCustomerId =>
        _httpAccessor.HttpContext?.User.FindFirst(SecurityNames.Actor)?.Value == "customer"
            ? SecurityNames.Id(_httpAccessor.HttpContext.User, SecurityNames.Customer)
            : null;

    private void ConfigureSecurityFilters(ModelBuilder builder)
    {
        builder.Entity<DropProduct>().HasQueryFilter(DropProduct =>
            !DropProduct.IsDeleted &&
            !DropProduct.DropEvent.IsDeleted &&
            DropProduct.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId());

        builder.Entity<DropCoupon>().HasQueryFilter(DropCoupon =>
            !DropCoupon.IsDeleted &&
            !DropCoupon.DropEvent.IsDeleted &&
            DropCoupon.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId());

        builder.Entity<DropRegistration>().HasQueryFilter(DropRegistration =>
            !DropRegistration.IsDeleted &&
            !DropRegistration.DropEvent.IsDeleted &&
            DropRegistration.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropRegistration.CustomerId == CurrentCustomerId));

        builder.Entity<DropReservation>().HasQueryFilter(DropReservation =>
            !DropReservation.IsDeleted &&
            !DropReservation.DropEvent.IsDeleted &&
            DropReservation.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropReservation.CustomerId == CurrentCustomerId));

        builder.Entity<DropOrder>().HasQueryFilter(DropOrder =>
            !DropOrder.IsDeleted &&
            !DropOrder.DropEvent.IsDeleted &&
            DropOrder.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropOrder.CustomerId == CurrentCustomerId));

        builder.Entity<DropNotification>().HasQueryFilter(DropNotification =>
            !DropNotification.IsDeleted &&
            !DropNotification.DropEvent.IsDeleted &&
            DropNotification.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropNotification.CustomerId == CurrentCustomerId));

        builder.Entity<QueueEntry>().HasQueryFilter(QueueEntry =>
            !QueueEntry.DropEvent.IsDeleted &&
            QueueEntry.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || QueueEntry.CustomerId == CurrentCustomerId));

        builder.Entity<QueueSession>().HasQueryFilter(QueueSession =>
            !QueueSession.QueueEntry.DropEvent.IsDeleted &&
            QueueSession.QueueEntry.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || QueueSession.CustomerId == CurrentCustomerId));

        builder.Entity<WaitlistEntry>().HasQueryFilter(WaitlistEntry =>
            !WaitlistEntry.DropEvent.IsDeleted &&
            WaitlistEntry.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || WaitlistEntry.CustomerId == CurrentCustomerId));

        builder.Entity<FraudSignal>().HasQueryFilter(FraudSignal =>
            !FraudSignal.DropEvent.IsDeleted &&
            FraudSignal.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || FraudSignal.CustomerId == CurrentCustomerId));

        builder.Entity<DropAuditLog>().HasQueryFilter(DropAuditLog =>
            !DropAuditLog.DropEvent.IsDeleted &&
            DropAuditLog.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId());

        builder.Entity<DropOrderItem>().HasQueryFilter(DropOrderItem =>
            !DropOrderItem.DropOrder.IsDeleted &&
            !DropOrderItem.DropOrder.DropEvent.IsDeleted &&
            DropOrderItem.DropOrder.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropOrderItem.DropOrder.CustomerId == CurrentCustomerId));

        builder.Entity<DropTransaction>().HasQueryFilter(DropTransaction =>
            !DropTransaction.DropOrder.IsDeleted &&
            !DropTransaction.DropOrder.DropEvent.IsDeleted &&
            DropTransaction.DropOrder.DropEvent.EnterpriseId == _tenantProvider.GetEnterpriseId() &&
            (CurrentCustomerId == null || DropTransaction.CustomerId == CurrentCustomerId));
    }

    private async Task GuardWritesAsync(CancellationToken ct)
    {
        var changes = ChangeTracker.Entries<BaseEntity>()
            .Where(EntityEntry =>
                EntityEntry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        if (changes.Count == 0)
            return;

        var principal = _httpAccessor.HttpContext?.User ?? throw new SecurityFailure(401, "Security.AuthenticationRequired", "Autenticação necessária.");

        var tenant = _tenantProvider.GetEnterpriseId();

        foreach (var entry in changes)
        {
            var properties = entry.Entity.GetType().GetProperties();

            if (entry.Entity is DropEvent DropEventEntity && DropEventEntity.EnterpriseId != tenant)
                throw new SecurityFailure(403, "Security.TenantMismatch", "Evento não pertence à loja autenticada.");


            if (properties.FirstOrDefault(Property => Property.Name == "DropEventId")?.GetValue(entry.Entity) is long eventId && !await DropEvents.AnyAsync(DropEvent => DropEvent.Id == eventId && DropEvent.EnterpriseId == tenant, ct))
                throw new SecurityFailure(400, "Security.InvalidEvent", "Evento indisponível ou de outra loja.");

            var customerId = properties
                .FirstOrDefault(Property => Property.Name == "CustomerId")
                ?.GetValue(entry.Entity) as long?;

            if (entry.State != EntityState.Deleted && customerId is long canonicalCustomerId && (await _storeCustomers.GetByIdAsync(canonicalCustomerId, tenant, ct)) is not { IsActive: true })
                throw new SecurityFailure(400, "Security.CustomerUnavailable", "Cliente indisponível ou de outra loja.");

            if (entry.State != EntityState.Deleted && properties.FirstOrDefault(Property => Property.Name == "ProductId")?.GetValue(entry.Entity) is long canonicalProductId && (await _storeProducts.GetByIdAsync(canonicalProductId, tenant, ct)) is not { IsActive: true })
                throw new SecurityFailure(400, "Security.ProductUnavailable", "Produto indisponível ou de outra loja.");

            if (principal.FindFirst(SecurityNames.Actor)?.Value == "customer")
            {
                if (!_operationScope.IsOpen || customerId is not null && customerId != SecurityNames.Id(principal, SecurityNames.Customer))
                    throw new SecurityFailure(403, "Security.CustomerMismatch", "Operação não pertence ao cliente autenticado.");
            }

            if (entry.State == EntityState.Deleted)
                continue;

            long? relationEvent = properties
                .FirstOrDefault(Property => Property.Name == "DropEventId")
                ?.GetValue(entry.Entity) as long?;

            long? relationCustomer = customerId;

            foreach (var relation in new[]
            {
                ("DropProductId", typeof(DropProduct)),
                ("DropCouponId", typeof(DropCoupon)),
                ("QueueEntryId", typeof(QueueEntry)),
                ("DropReservationId", typeof(DropReservation)),
                ("DropOrderId", typeof(DropOrder))
            })
            {
                if (properties.FirstOrDefault(Property => Property.Name == relation.Item1)?.GetValue(entry.Entity) is not long id)
                    continue;

                var target = await FindAsync(relation.Item2, [id], ct) as BaseEntity;

                if (target is null || target is ISoftDeletable { IsDeleted: true })
                    throw new SecurityFailure(400, "Security.InvalidRelationship", "Referência inexistente ou de outra loja.");

                var targetProperties = target.GetType().GetProperties();

                var targetEvent = targetProperties
                    .FirstOrDefault(Property => Property.Name == "DropEventId")
                    ?.GetValue(target) as long?;

                var targetCustomer = targetProperties
                    .FirstOrDefault(Property => Property.Name == "CustomerId")
                    ?.GetValue(target) as long?;

                if (targetEvent is not null && relationEvent is not null && targetEvent != relationEvent || targetCustomer is not null && relationCustomer is not null && targetCustomer != relationCustomer)
                    throw new SecurityFailure(400, "Security.InvalidRelationship", "Referências pertencem a eventos ou clientes diferentes.");

                relationEvent ??= targetEvent;
                relationCustomer ??= targetCustomer;

                if (targetEvent is not null && !await DropEvents.AnyAsync(DropEvent => DropEvent.Id == targetEvent && DropEvent.EnterpriseId == tenant, ct))
                    throw new SecurityFailure(403, "Security.TenantMismatch", "Referência não pertence à loja autenticada.");
            }
        }
    }
}
