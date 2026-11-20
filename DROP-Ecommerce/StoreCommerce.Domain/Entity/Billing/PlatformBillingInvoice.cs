using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public sealed class PlatformBillingInvoice : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long SubscriptionId { get; private set; }
    public long OfferId { get; private set; }
    public string Origin { get; private set; } = string.Empty;
    public bool IsUpgrade { get; private set; }
    public string Reference { get; private set; } = string.Empty;
    public decimal Amount { get; private set; }
    public decimal MonthlyAmount { get; private set; }
    public string Currency { get; private set; } = "BRL";
    public DateTime PeriodStart { get; private set; }
    public DateTime PeriodEnd { get; private set; }
    public DateTime CheckoutExpiresAt { get; private set; }
    public DateTime? SubmittedAt { get; private set; }
    public string? PreferenceId { get; private set; }
    public string? CheckoutUrl { get; private set; }
    public string? PaymentId { get; private set; }
    public DateTime? PaymentUpdatedAt { get; private set; }
    public string Status { get; private set; } = "pending";
    public DateTime? AppliedAt { get; private set; }
    public string? ReviewCode { get; private set; }

    private PlatformBillingInvoice()
    {
    }

    public static PlatformBillingInvoice Create(PlatformSubscription subscription, EnterpriseContractChange offer,
        string origin, bool upgrade, decimal amount, DateTime start, DateTime end, DateTime checkoutExpiresAt)
    {
        if (offer.EnterpriseId != subscription.EnterpriseId || amount <= 0 || decimal.Round(amount, 2) != amount || end <= start)
            throw new ArgumentException("Fatura inválida.");

        return new PlatformBillingInvoice
        {
            EnterpriseId = subscription.EnterpriseId,
            SubscriptionId = subscription.Id,
            OfferId = offer.Id,
            Origin = origin,
            IsUpgrade = upgrade,
            Reference = "drp-saas-" + Guid.NewGuid().ToString("N"),
            Amount = amount,
            MonthlyAmount = offer.PlanMonthlyPrice + offer.AddOnMonthlyPrice,
            Currency = offer.Currency,
            PeriodStart = start,
            PeriodEnd = end,
            CheckoutExpiresAt = checkoutExpiresAt
        };
    }

    public void Submit(DateTime now)
    {
        if (SubmittedAt.HasValue)
            throw new InvalidOperationException("A criação da cobrança já foi submetida.");

        SubmittedAt = now;
    }

    public void Checkout(string preferenceId, string checkoutUrl)
    {
        if (PreferenceId is not null && PreferenceId != preferenceId)
            throw new InvalidOperationException("A preferência não pode ser substituída.");

        PreferenceId = preferenceId;
        CheckoutUrl = checkoutUrl;
    }

    public void Observe(string paymentId, string status, DateTime updatedAt)
    {
        if (PaymentId == paymentId && PaymentUpdatedAt.HasValue && PaymentUpdatedAt.Value > updatedAt)
            return;

        if (PaymentId is not null && PaymentId != paymentId && Status == "approved" && status != "approved")
            return;

        if (PaymentId is not null && PaymentId != paymentId && Status == "approved")
            throw new InvalidOperationException("Há mais de um pagamento aprovado para a fatura.");

        PaymentId = paymentId;
        Status = status;
        PaymentUpdatedAt = updatedAt;
    }

    public void ExpireCheckout()
    {
        if (Status != "approved")
            Status = "expired";
    }

    public void Applied(DateTime now)
    {
        AppliedAt ??= now;
    }

    public void Review(string code)
    {
        ReviewCode = code;
    }
}
