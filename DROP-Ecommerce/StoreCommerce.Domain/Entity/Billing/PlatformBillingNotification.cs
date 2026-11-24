using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Entity;

public sealed class PlatformBillingNotification : BaseEntity
{
    public string Delivery { get; private set; } = string.Empty;
    public string Topic { get; private set; } = string.Empty;
    public string ResourceId { get; private set; } = string.Empty;
    public string AccountId { get; private set; } = string.Empty;
    public IntegrationEnvironment Environment { get; private set; }
    public DateTime NextCheckAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    private PlatformBillingNotification()
    {
    }

    public static PlatformBillingNotification Receive(string delivery, string topic, string resourceId,
        string accountId, IntegrationEnvironment environment, DateTime now)
    {
        return new PlatformBillingNotification
        {
            Delivery = delivery,
            Topic = topic,
            ResourceId = resourceId,
            AccountId = accountId,
            Environment = environment,
            NextCheckAt = now
        };
    }

    public void Complete(DateTime now)
    {
        CompletedAt = now;
    }

    public void Retry(DateTime next)
    {
        NextCheckAt = next;
    }
}
