using StoreCommerce.Domain.Enums.Inventory;
using StoreCommerce.Domain.Interfaces.Tenancy;
using StoreCommerce.Domain.Entity.Base;

namespace StoreCommerce.Domain.Entity;

public class StockReservation : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public string Reference { get; private set; } = string.Empty;
    public StockReservationStatus Status { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public long Version { get; private set; } = 1;
    private readonly List<StockReservationLine> _lines = [];
    public IReadOnlyCollection<StockReservationLine> Lines => _lines.AsReadOnly();

    protected StockReservation()
    {
    }

    public static StockReservation Create(long tenant, string key, string reference, DateTime expiresAt)
    {
        BaseValidate.ValidatePositive(tenant, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(key, nameof(IdempotencyKey));
        BaseValidate.ValidateMaxLength(key, 100, nameof(IdempotencyKey));
        BaseValidate.ValidateMaxLength(reference, 200, nameof(Reference));

        return new StockReservation
        {
            EnterpriseId = tenant,
            IdempotencyKey = key.Trim(),
            Reference = reference?.Trim() ?? string.Empty,
            ExpiresAt = expiresAt,
            Status = StockReservationStatus.Reserved
        };
    }

    public void AddLine(ProductVariant variant, int quantity, bool allowBackorder)
    {
        if (Status != StockReservationStatus.Reserved || variant.EnterpriseId != EnterpriseId ||
            _lines.Any(Line => Line.ProductVariantId == variant.Id))
            throw new ArgumentException("Linha de reserva inválida ou duplicada.");

        BaseValidate.ValidateRange(quantity, 1, 10000, nameof(quantity));
        _lines.Add(StockReservationLine.Create(this, variant, quantity, allowBackorder));
    }

    public void Complete(StockReservationStatus status, DateTime now)
    {
        if (Status != StockReservationStatus.Reserved || status == StockReservationStatus.Reserved)
            throw new ArgumentException("A reserva já foi concluída.");

        Status = status;
        CompletedAt = now;
        Version = checked(Version + 1);
    }
}
