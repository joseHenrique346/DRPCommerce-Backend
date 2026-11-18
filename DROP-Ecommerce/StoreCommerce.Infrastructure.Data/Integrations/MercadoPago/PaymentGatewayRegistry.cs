using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class PaymentGatewayRegistry(IEnumerable<IPaymentGateway> gateways, StoreCommerceDbContext context,
    IntegrationWriteScope scope) : IPaymentGatewayRegistry
{
    public IPaymentGateway Resolve(string provider)
    {
        return gateways.LastOrDefault(Gateway => Gateway.Provider == provider)
            ?? throw new SecurityFailure(409, "Payment.ProviderUnavailable", "O provedor original do pagamento está indisponível.");
    }

    public async Task<PaymentProviderContext> GetContextAsync(long enterpriseId, CancellationToken ct)
    {
        var selection = await context.Set<PaymentProviderSelection>().IgnoreQueryFilters().AsNoTracking()
            .SingleOrDefaultAsync(Selection => Selection.EnterpriseId == enterpriseId, ct);
        var gateway = Resolve(selection?.Provider ?? IntegrationProviders.Manual);

        if (gateway is MercadoPagoGateway mercadoPago)
            return await mercadoPago.GetContextAsync(enterpriseId, selection!.Environment, ct);

        return await gateway.GetContextAsync(enterpriseId, ct);
    }

    public async Task SelectAsync(long enterpriseId, string provider, IntegrationEnvironment environment, CancellationToken ct)
    {
        // Validar a conexão antes de selecionar. Uma desconexão posterior não muda silenciosamente para pagamento manual.
        if (Resolve(provider) is MercadoPagoGateway mercadoPago)
            await mercadoPago.GetContextAsync(enterpriseId, environment, ct);

        var selection = await context.Set<PaymentProviderSelection>().IgnoreQueryFilters()
            .SingleOrDefaultAsync(Selection => Selection.EnterpriseId == enterpriseId, ct);

        if (selection is null)
            context.Add(PaymentProviderSelection.Create(enterpriseId, provider, environment));
        else
            selection.Select(provider, environment);

        using (scope.Begin(enterpriseId))
            await context.SaveChangesAsync(ct);
    }
}
