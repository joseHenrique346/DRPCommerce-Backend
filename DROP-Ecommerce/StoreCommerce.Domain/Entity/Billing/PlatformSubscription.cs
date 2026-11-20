using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public sealed class PlatformSubscription : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long OfferId { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public string? RemoteId { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public string RemoteStatus { get; private set; } = "pending";
    public DateTime? PeriodStart { get; private set; }
    public DateTime? BillingAnchor { get; private set; }
    public DateTime? PaidUntil { get; private set; }
    public long AppliedContractRevision { get; private set; }
    public long? PendingOfferId { get; private set; }
    public DateTime? ChangeEffectiveAt { get; private set; }
    public bool IsUpgrade { get; private set; }
    public bool ChangeSynchronized { get; private set; }
    public string? RetainedMembershipIdsJson { get; private set; }

    [System.ComponentModel.DataAnnotations.Schema.NotMapped]
    public IReadOnlyList<long>? RetainedMembershipIds => RetainedMembershipIdsJson is null ? null :
        System.Text.Json.JsonSerializer.Deserialize<long[]>(RetainedMembershipIdsJson);
    public string? ReviewCode { get; private set; }
    public DateTime NextCheckAt { get; private set; }
    public DateTime? ClosedAt { get; private set; }

    private PlatformSubscription()
    {
    }

    public static PlatformSubscription Prepare(EnterpriseContractChange offer, string account,
        IntegrationEnvironment environment, DateTime now)
    {
        if (offer.PlanMonthlyPrice + offer.AddOnMonthlyPrice <= 0 || !Enum.IsDefined(environment))
            throw new ArgumentException("Uma assinatura exige oferta paga e ambiente válido.");

        return new PlatformSubscription
        {
            EnterpriseId = offer.EnterpriseId,
            OfferId = offer.Id,
            Reference = "drp-saas-" + Guid.NewGuid().ToString("N"),
            AccountId = account,
            Environment = environment,
            AppliedContractRevision = offer.ExpectedContractRevision,
            RetainedMembershipIdsJson = offer.RetainedMembershipIdsJson,
            NextCheckAt = now
        };
    }

    public void Submit(DateTime now)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("A criação da assinatura já foi submetida.");

        SubmittedAt = now;
    }

    public void ObserveRemote(string id, string? checkoutUrl, string status)
    {
        if (RemoteId is not null && RemoteId != id)
            throw new InvalidOperationException("A assinatura externa não pode ser substituída.");

        RemoteId = id;
        CheckoutUrl = checkoutUrl ?? CheckoutUrl;
        RemoteStatus = status;
    }

    public void Schedule(EnterpriseContractChange offer, DateTime effectiveAt, bool upgrade)
    {
        if (PendingOfferId.HasValue || offer.EnterpriseId != EnterpriseId)
            throw new InvalidOperationException("Há uma mudança pendente ou a oferta é de outra loja.");

        PendingOfferId = offer.Id;
        ChangeEffectiveAt = effectiveAt;
        IsUpgrade = upgrade;
        ChangeSynchronized = false;
        RetainedMembershipIdsJson = offer.RetainedMembershipIdsJson;
    }

    public void SelectTeam(IReadOnlyList<long>? retained)
    {
        RetainedMembershipIdsJson = retained is null ? null :
            System.Text.Json.JsonSerializer.Serialize(retained.OrderBy(Id => Id));
    }

    public void SynchronizeChange()
    {
        ChangeSynchronized = true;
    }

    public void AbandonUpgrade(IReadOnlyList<long>? retained)
    {
        if (!IsUpgrade)
            throw new InvalidOperationException("A mudança não é um upgrade pendente.");

        PendingOfferId = null;
        ChangeEffectiveAt = null;
        IsUpgrade = false;
        ChangeSynchronized = false;
        SelectTeam(retained);
    }

    public void ApplyOffer(long offerId)
    {
        OfferId = offerId;

        if (PendingOfferId == offerId)
        {
            PendingOfferId = null;
            ChangeEffectiveAt = null;
            IsUpgrade = false;
            ChangeSynchronized = false;
        }
    }

    public void Renew(DateTime start, DateTime end)
    {
        if (start.Kind != DateTimeKind.Utc || end.Kind != DateTimeKind.Utc || end <= start)
            throw new ArgumentException("Vigência inválida.");

        if (PaidUntil.HasValue && end <= PaidUntil.Value)
            return;

        PeriodStart = start;
        BillingAnchor ??= start;
        PaidUntil = end;
    }

    public void Applied(long revision)
    {
        AppliedContractRevision = revision;
    }

    public void Review(string code)
    {
        ReviewCode = code;
    }

    public void CheckAfter(DateTime next)
    {
        NextCheckAt = next;
    }

    public void Close(DateTime now)
    {
        ClosedAt = now;
    }
}
