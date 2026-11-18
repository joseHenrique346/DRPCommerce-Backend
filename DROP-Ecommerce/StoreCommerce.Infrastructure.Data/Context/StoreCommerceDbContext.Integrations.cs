using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    public DbSet<ProviderConnection> ProviderConnections => Set<ProviderConnection>();
    public DbSet<PaymentAttempt> PaymentAttempts => Set<PaymentAttempt>();
    public DbSet<PaymentExternalPayment> PaymentExternalPayments => Set<PaymentExternalPayment>();
    public DbSet<PaymentOperation> PaymentOperations => Set<PaymentOperation>();

    private bool GuardIntegrationWrites(List<EntityEntry> changes)
    {
        if (_integrationWrites.EnterpriseId is not long tenant)
            return false;

        foreach (var entry in changes)
        {
            if (entry.State == EntityState.Deleted || entry.Entity is not ITenantEntity entity || entity.EnterpriseId != tenant ||
                entry.Entity is not (ProviderConnection or PaymentAttempt or PaymentExternalPayment))
                throw new SecurityFailure(403, "Integration.InvalidScope", "Gravação fora do escopo da integração.");
        }

        return true;
    }
}
