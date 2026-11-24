using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations.MelhorEnvio;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Enums.Orders;
using StoreCommerce.Domain.Interfaces.Identity;
using StoreCommerce.Domain.Interfaces.Orders;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetShippingQuotesQueryHandler(
    CartPurchaseContextReader context,
    ICartRepository carts,
    ICustomerRepository customers,
    CheckoutAddressReader addresses,
    CheckoutShippingConnectionReader connections,
    IMelhorEnvioQuoteClient provider,
    CheckoutShippingQuoteProtector protector,
    TimeProvider time)
    : WorkflowRequestHandler<GetShippingQuotesQuery, CheckoutShippingQuotesResponse>
{
    protected override async Task<CheckoutShippingQuotesResponse> ExecuteAsync(GetShippingQuotesQuery query, CancellationToken ct)
    {
        var purchase = await context.ReadAsync(ct);
        var cart = await carts.GetForCustomerAsync(purchase.EnterpriseId, purchase.CustomerId, tracking: false, ct);
        var selected = cart?.ListCartItem.Where(Item => Item.IsSelected).ToList() ?? [];

        if (selected.Count == 0)
            throw new SecurityFailure(400, "Checkout.NoItemsSelected", "Selecione ao menos um item do carrinho para cotar o frete.");

        var settings = purchase.Settings;

        if (selected.Any(Item => Item.GetAvailability(settings.AllowBackorder, purchase.VisibleCategories) != CartItemAvailability.Available))
            throw new SecurityFailure(409, "Checkout.ItemsUnavailable", "Atualize os itens selecionados antes de cotar o frete.");

        if (!settings.IsWithinItemLimit(selected.Sum(Item => Item.Quantity)))
            throw new SecurityFailure(400, "Checkout.MaxItemsExceeded", "O carrinho excede o limite de itens da loja.");

        if (selected.All(Item => Item.Product.IsDigital))
            return new CheckoutShippingQuotesResponse(false, false, settings.CurrencyCode, null, []);

        var connection = await connections.FindAsync(purchase.EnterpriseId, ct);

        if (connection is null)
            return new CheckoutShippingQuotesResponse(false, settings.AllowPickup, settings.CurrencyCode, null, []);

        var customer = await customers.FindAsync(Customer => Customer.Id == purchase.CustomerId &&
            Customer.EnterpriseId == purchase.EnterpriseId && Customer.IsActive && !Customer.IsDeleted, ct, tracking: false)
            ?? throw new SecurityFailure(401, "Identity.InvalidCredentials", "A conta não está disponível para novas compras.");
        var origin = await addresses.StoreAsync(purchase.EnterpriseId, ct);
        var destination = await addresses.ResolveAsync(query.Request, customer, ct);
        var input = CheckoutShippingQuoteInput.Create(selected, destination, origin.ZipCode, settings.CurrencyCode);
        var quotes = await provider.CalculateAsync(connection, input.OriginPostalCode, input.DestinationPostalCode, input.Products, ct);
        var now = time.GetUtcNow().UtcDateTime;
        var expiresAt = now.AddMinutes(15);
        var options = new List<CheckoutShippingOptionResponse>();

        foreach (var quote in quotes)
        {
            var snapshot = new OrderShippingSnapshot(connection.Provider, connection.Environment, connection.Id, connection.ConnectedAt,
                input.OriginPostalCode, input.DestinationPostalCode, now, expiresAt, quote);
            var token = protector.Protect(purchase.EnterpriseId, purchase.CustomerId, new CheckoutShippingQuoteToken(input.Fingerprint, snapshot));

            if (token.Length <= CheckoutShippingQuoteProtector.MaxTokenLength)
                options.Add(new CheckoutShippingOptionResponse(quote.ServiceId, quote.ServiceName, quote.CarrierName, quote.Price,
                    quote.DeliveryDays, token));
        }

        return new CheckoutShippingQuotesResponse(true, settings.AllowPickup, settings.CurrencyCode, expiresAt, options);
    }
}
