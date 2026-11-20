using System.Text.Json;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

/// <summary>
/// Trilha comercial acrescentável pelo servidor; snapshots sobrevivem à exclusão lógica do catálogo.
/// </summary>
public sealed class StoreActivity : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long? ProductId { get; private set; }
    public long? ProductVariantId { get; private set; }
    public long? ActorUserId { get; private set; }
    public string ActorType { get; private set; } = "system";
    public string EventType { get; private set; } = string.Empty;
    public string ReasonCode { get; private set; } = string.Empty;
    public string CorrelationId { get; private set; } = string.Empty;
    public string ProductName { get; private set; } = string.Empty;
    public string SKU { get; private set; } = string.Empty;
    public string DetailsJson { get; private set; } = "{}";
    public DateTime OccurredAt { get; private set; }
    public Product? Product { get; private set; }
    public ProductVariant? ProductVariant { get; private set; }

    private StoreActivity()
    {
    }

    public static StoreActivity Record(long enterpriseId, string eventType, Product? product,
        ProductVariant? variant, long? actorUserId, string reasonCode, string correlationId,
        object details, DateTime now)
    {
        if (enterpriseId <= 0 || string.IsNullOrWhiteSpace(eventType) || eventType.Length > 60 ||
            now.Kind != DateTimeKind.Utc || product is not null && product.EnterpriseId != enterpriseId ||
            variant is not null && variant.EnterpriseId != enterpriseId)
            throw new ArgumentException("Evento de atividade inválido.");

        return new StoreActivity
        {
            EnterpriseId = enterpriseId,
            Product = product,
            ProductId = product?.Id,
            ProductVariant = variant,
            ProductVariantId = variant?.Id,
            ActorUserId = actorUserId,
            ActorType = actorUserId.HasValue ? "user" : "system",
            EventType = eventType,
            ReasonCode = reasonCode,
            CorrelationId = correlationId.Length > 128 ? correlationId[..128] : correlationId,
            ProductName = product?.Name ?? string.Empty,
            SKU = variant?.SKU ?? product?.SKU ?? string.Empty,
            DetailsJson = JsonSerializer.Serialize(details),
            OccurredAt = now
        };
    }
}
