using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Interfaces.Tenancy;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Caixa de entrada durável: não armazena o corpo do webhook nem confia no estado financeiro enviado pelo navegador.
/// </summary>
public sealed class ProviderNotification : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long ProviderConnectionId { get; private set; }
    public long? PaymentAttemptId { get; private set; }
    public string PaymentId { get; private set; } = string.Empty;
    public string DeliveryHash { get; private set; } = string.Empty;
    public ProviderNotificationStatus Status { get; private set; }
    public string? ResolutionCode { get; private set; }
    public DateTime NextAttemptAt { get; private set; }
    public Guid? LeaseToken { get; private set; }
    public DateTime? LeaseExpiresAt { get; private set; }
    public long Version { get; private set; }
    public ProviderConnection Connection { get; private set; } = null!;

    private ProviderNotification()
    {
    }

    public static ProviderNotification Create(ProviderConnection connection, string paymentId, string deliveryHash, DateTime now,
        long? paymentAttemptId = null)
    {
        if (string.IsNullOrWhiteSpace(paymentId) || paymentId.Length > 200 || deliveryHash.Length != 64)
            throw new ArgumentException("Notificação inválida.");

        return new ProviderNotification
        {
            EnterpriseId = connection.EnterpriseId,
            ProviderConnectionId = connection.Id,
            PaymentAttemptId = paymentAttemptId,
            PaymentId = paymentId,
            DeliveryHash = deliveryHash,
            Status = ProviderNotificationStatus.Pending,
            NextAttemptAt = now
        };
    }
}
