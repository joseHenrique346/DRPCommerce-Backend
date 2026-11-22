namespace StoreCommerce.Application.Billing;

public interface IPlatformBillingSignature
{
    string Verify(string paymentId, string requestId, string signature);
}
