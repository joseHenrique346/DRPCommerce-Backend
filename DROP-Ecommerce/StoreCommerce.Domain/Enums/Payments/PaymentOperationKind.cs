using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Domain.Enums.Payments;

public enum PaymentOperationKind
{
    Cancel = 1,
    FullRefund = 2
}
