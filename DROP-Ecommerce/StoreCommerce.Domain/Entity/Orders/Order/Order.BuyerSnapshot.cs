using StoreCommerce.Domain.Enums.Orders;

namespace StoreCommerce.Domain.Entity;

public partial class Order
{
    public string BuyerName { get; private set; } = string.Empty;
    public string BuyerEmail { get; private set; } = string.Empty;
    public string BuyerPhone { get; private set; } = string.Empty;
    public DateTime? BuyerSnapshotCapturedAt { get; private set; }
    public BuyerSnapshotSource? BuyerSnapshotSource { get; private set; }

    /// <summary>
    /// Dados comerciais do comprador na criação do pedido. Alterações posteriores de perfil não reescrevem a venda.
    /// Snapshots legados precisam indicar que vieram do cadastro atual, sem alegar recuperar o passado.
    /// </summary>
    public void CaptureBuyerSnapshot(Customer customer, DateTime capturedAt, BuyerSnapshotSource source)
    {
        if (BuyerSnapshotCapturedAt.HasValue)
            return;

        if (customer.Id != CustomerId || customer.EnterpriseId != EnterpriseId || !Enum.IsDefined(source))
            throw new ArgumentException("Comprador incompatível com o pedido.");

        BuyerName = customer.FullName;
        BuyerEmail = customer.Email.Value;
        BuyerPhone = customer.Phone.Value;
        BuyerSnapshotCapturedAt = capturedAt;
        BuyerSnapshotSource = source;
    }
}
