using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    /// <summary>
    /// Endereços são gravados pelo próprio cliente na vitrine. Com o escopo aberto, apenas os endereços do cliente autenticado
    /// na loja atual podem mudar, sem trocar o dono; sem o escopo, nenhum fluxo altera endereços.
    /// </summary>
    private bool GuardCustomerAddressWrites(List<EntityEntry> changes)
    {
        if (_customerAddressWrites.EnterpriseId is not long tenant || _customerAddressWrites.CustomerId is not long customer)
        {
            if (changes.Any(entry => entry.Entity is CustomerAddress))
                throw new SecurityFailure(403, "CustomerAddress.UseWorkflow", "Use os fluxos de endereço do cliente.");

            return false;
        }

        if (tenant != _tenantProvider.GetEnterpriseId())
            throw new SecurityFailure(403, "Security.TenantMismatch", "Endereço não pertence à loja atual.");

        foreach (var entry in changes)
        {
            if (entry.Entity is not CustomerAddress address || address.EnterpriseId != tenant || address.CustomerId != customer ||
                (entry.State == EntityState.Modified && (entry.Property(nameof(CustomerAddress.CustomerId)).IsModified ||
                    entry.Property(nameof(CustomerAddress.EnterpriseId)).IsModified)))
                throw new SecurityFailure(403, "CustomerAddress.InvalidScope", "O endereço não pertence ao cliente autenticado.");
        }

        return true;
    }
}
