using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Infrastructure.Data.Context;

public partial class StoreCommerceDbContext
{
    private void GuardContractWrites()
    {
        foreach (var entry in ChangeTracker.Entries()
            .Where(Entry => Entry.State is EntityState.Added or EntityState.Modified or EntityState.Deleted))
        {
            if (entry.Entity is EnterprisePlan or EnterpriseAddOn or EnterprisePlanAddOn)
                throw new SecurityFailure(403, "Contract.CatalogReadOnly", "O catálogo comercial é mantido pela plataforma.");

            if (entry.Entity is EnterpriseContractChange && !_identityScope.IsOpen)
                throw new SecurityFailure(403, "Contract.UseWorkflow", "Use o fluxo de solicitação de contrato.");

            if (entry.Entity is not Enterprise enterprise)
                continue;

            if (entry.State == EntityState.Added &&
                (enterprise.PlanId != EnterprisePlan.Free.Id || enterprise.AddOnId.HasValue ||
                 enterprise.ContractRevision != 1 || enterprise.CatalogVersion != EnterpriseCommercialCatalog.Version))
                throw GrantRequired();

            if (entry.State != EntityState.Modified)
                continue;

            var changed = entry.Property(nameof(Enterprise.PlanId)).IsModified ||
                entry.Property(nameof(Enterprise.AddOnId)).IsModified ||
                entry.Property(nameof(Enterprise.ContractRevision)).IsModified ||
                entry.Property(nameof(Enterprise.CatalogVersion)).IsModified;

            if (!changed)
                continue;

            if (_contractWrites.EnterpriseId != enterprise.Id)
                throw GrantRequired();

            var oldRevision = (long)entry.Property(nameof(Enterprise.ContractRevision)).OriginalValue!;

            if (enterprise.ContractRevision != checked(oldRevision + 1) ||
                enterprise.CatalogVersion != EnterpriseCommercialCatalog.Version)
                throw new SecurityFailure(409, "Contract.InvalidRevision", "Revisão do contrato inválida.");

            try
            {
                EnterpriseEntitlements.Resolve(enterprise.PlanId, enterprise.AddOnId);
            }
            catch (ArgumentException)
            {
                throw new SecurityFailure(400, "Contract.InvalidOffer", "Plano ou adicional inválido.");
            }
        }
    }

    private static SecurityFailure GrantRequired()
    {
        return new SecurityFailure(403, "Contract.GrantRequired",
            "A escolha do plano não concede acesso. Use um fluxo autorizado de concessão.");
    }
}
