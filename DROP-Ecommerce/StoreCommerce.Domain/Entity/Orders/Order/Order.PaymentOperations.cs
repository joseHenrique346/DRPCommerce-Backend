namespace StoreCommerce.Domain.Entity;

public partial class Order
{
    private readonly List<PaymentOperation> _listPaymentOperation = [];
    public IReadOnlyCollection<PaymentOperation> ListPaymentOperation => _listPaymentOperation;
}
