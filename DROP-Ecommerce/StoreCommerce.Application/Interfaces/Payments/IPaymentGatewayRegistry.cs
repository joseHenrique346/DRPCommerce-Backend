using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Application.Interfaces.Payments;

public interface IPaymentGatewayRegistry
{
    IPaymentGateway Resolve(string provider);
    /// <summary>
    /// Seleção/configuração local, sem HTTP ao provedor. Pode ser consultada na transação comercial.
    /// </summary>
    Task<PaymentProviderContext> GetContextAsync(long enterpriseId, CancellationToken ct);
    Task SelectAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct);
}
