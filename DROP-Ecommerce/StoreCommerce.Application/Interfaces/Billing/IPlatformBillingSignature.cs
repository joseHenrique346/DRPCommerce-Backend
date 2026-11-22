using StoreCommerce.Application.Billing;

namespace StoreCommerce.Application.Interfaces.Billing;

public interface IPlatformBillingSignature
{
    string Verify(string paymentId, string requestId, string signature);
}
