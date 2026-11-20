using StoreCommerce.Domain.Contracts;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;
using System.Text.Json;
using System.ComponentModel.DataAnnotations.Schema;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Intenção do dono. Solicitar uma oferta não altera o contrato efetivo nem comprova pagamento.
/// </summary>
public sealed class EnterpriseContractChange : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long? RequestedByUserId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public long PlanId { get; private set; }
    public long? AddOnId { get; private set; }
    public long ExpectedContractRevision { get; private set; }
    public int CatalogVersion { get; private set; }
    public decimal PlanMonthlyPrice { get; private set; }
    public decimal AddOnMonthlyPrice { get; private set; }
    public string Currency { get; private set; } = EnterpriseCommercialCatalog.Currency;
    public EnterpriseContractChangeStatus Status { get; private set; }
    public DateTime RequestedAt { get; private set; }
    public DateTime? SupersededAt { get; private set; }
    public string? RetainedMembershipIdsJson { get; private set; }

    [NotMapped]
    public IReadOnlyList<long>? RetainedMembershipIds => RetainedMembershipIdsJson is null
        ? null : JsonSerializer.Deserialize<long[]>(RetainedMembershipIdsJson);

    private EnterpriseContractChange()
    {
    }

    public static EnterpriseContractChange Request(long enterpriseId, long userId, string key,
        long planId, long? addOnId, long expectedRevision, DateTime now, IReadOnlyList<long>? retainedMembershipIds = null)
    {
        if (enterpriseId <= 0 || userId <= 0 || expectedRevision <= 0 ||
            string.IsNullOrWhiteSpace(key) || key.Trim().Length > 100 || now.Kind != DateTimeKind.Utc)
            throw new ArgumentException("Solicitação de contrato inválida.");

        var rights = EnterpriseEntitlements.Resolve(planId, addOnId);
        var plan = EnterprisePlan.FromId(rights.PlanId);
        var addOn = rights.AddOnId.HasValue ? EnterpriseAddOn.FromId(rights.AddOnId.Value) : null;

        if (retainedMembershipIds is not null && (retainedMembershipIds.Count > 10000 ||
            retainedMembershipIds.Any(Id => Id <= 0) || retainedMembershipIds.Distinct().Count() != retainedMembershipIds.Count))
            throw new ArgumentException("Seleção de equipe inválida.");

        return new EnterpriseContractChange
        {
            EnterpriseId = enterpriseId,
            RequestedByUserId = userId,
            IdempotencyKey = key.Trim(),
            PlanId = plan.Id,
            AddOnId = addOn?.Id,
            ExpectedContractRevision = expectedRevision,
            CatalogVersion = EnterpriseCommercialCatalog.Version,
            PlanMonthlyPrice = plan.MonthlyPrice,
            AddOnMonthlyPrice = addOn?.MonthlyPrice ?? 0m,
            Status = EnterpriseContractChangeStatus.Requested,
            RequestedAt = now,
            RetainedMembershipIdsJson = retainedMembershipIds is null ? null : JsonSerializer.Serialize(retainedMembershipIds.OrderBy(Id => Id))
        };
    }

    public void Supersede(DateTime now)
    {
        if (Status != EnterpriseContractChangeStatus.Requested || now.Kind != DateTimeKind.Utc || now < RequestedAt)
            throw new InvalidOperationException("A solicitação não está pendente.");

        Status = EnterpriseContractChangeStatus.Superseded;
        SupersededAt = now;
    }
}
