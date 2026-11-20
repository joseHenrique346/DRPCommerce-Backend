using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Infrastructure.Data.Billing;

/// <summary>
/// Adapter de pagamentos avulsos da plataforma. O workflow deve persistir a tentativa
/// antes do POST e usar RecoverAsync quando seu resultado for desconhecido, sem reenviar.
/// Não depende de conexão, pedido, transação ou credencial de uma loja.
/// </summary>
public sealed class MercadoPagoPlatformBillingProvider(MercadoPagoApi api,
    IOptions<PlatformBillingOptions> options, TimeProvider time) : IPlatformBillingProvider
{
    private PlatformBillingOptions Settings => options.Value;

    public async Task ValidateAccountAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        ValidateCharge(charge, false);
        await AccountAsync(ct);
    }

    public async Task<PlatformBillingCheckout> CreateAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        ValidateCharge(charge, true);
        Settings.RequireCheckout();
        await AccountAsync(ct);

        var returnUrl = Settings.ReturnUrl + (Settings.ReturnUrl.Contains('?') ? "&" : "?") +
            "billingReference=" + Uri.EscapeDataString(charge.ExternalReference);
        var body = new
        {
            items = new[]
            {
                new
                {
                    id = charge.ExternalReference,
                    title = "Plano DRPCommerce",
                    quantity = 1,
                    currency_id = charge.Currency,
                    unit_price = charge.Amount
                }
            },
            external_reference = charge.ExternalReference,
            notification_url = Settings.WebhookUrl,
            back_urls = new { success = returnUrl, pending = returnUrl, failure = returnUrl },
            auto_return = "approved",
            expires = true,
            expiration_date_from = time.GetUtcNow().UtcDateTime.AddMinutes(-1),
            expiration_date_to = charge.CheckoutExpiresAt
        };

        // A criação de preferências não oferece a garantia de idempotência dos refunds.
        // Nenhum retry de POST é efetuado por este adapter.
        using var json = await api.SendAsync(HttpMethod.Post, "/checkout/preferences",
            Settings.AccessToken, body, null, ct);

        return ReadCheckout(json.RootElement, charge);
    }

    public async Task<PlatformBillingCheckout?> RecoverAsync(PlatformBillingCharge charge, CancellationToken ct)
    {
        ValidateCharge(charge, false);
        await AccountAsync(ct);
        using var search = await api.SendAsync(HttpMethod.Get,
            "/checkout/preferences/search?external_reference=" + Uri.EscapeDataString(charge.ExternalReference) + "&limit=2",
            Settings.AccessToken, null, null, ct);
        var root = search.RootElement;

        if (!root.TryGetProperty("elements", out var elements) || elements.ValueKind != JsonValueKind.Array ||
            !root.TryGetProperty("total", out var total) || !total.TryGetInt32(out var count) || count < 0)
            throw InvalidResponse();

        if (count == 0 && elements.GetArrayLength() == 0)
            return null;

        if (count != 1 || elements.GetArrayLength() != 1)
            throw new SecurityFailure(409, "Billing.AmbiguousPreference", "A correlação financeira precisa de revisão.");

        var id = MercadoPagoApi.Id(elements[0], "id");
        using var preference = await api.SendAsync(HttpMethod.Get, "/checkout/preferences/" + Uri.EscapeDataString(id),
            Settings.AccessToken, null, null, ct);

        if (MercadoPagoApi.Id(preference.RootElement, "id") != id)
            throw Mismatch();

        return ReadCheckout(preference.RootElement, charge);
    }

    public async Task<PlatformBillingPayment> GetPaymentAsync(string paymentId, CancellationToken ct)
    {
        if (paymentId.Length is < 1 or > 40 || paymentId.Any(Character => !char.IsAsciiDigit(Character)))
            throw new SecurityFailure(400, "Billing.InvalidPaymentId", "Identificador de pagamento inválido.");

        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/v1/payments/" + paymentId,
            Settings.AccessToken, null, null, ct);
        var payment = ReadPayment(json.RootElement);

        if (payment.Id != paymentId)
            throw Mismatch();

        return payment;
    }

    public async Task<IReadOnlyList<PlatformBillingPayment>> SearchPaymentsAsync(string externalReference, CancellationToken ct)
    {
        ValidateReference(externalReference);
        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Get,
            "/v1/payments/search?external_reference=" + Uri.EscapeDataString(externalReference) + "&limit=100&sort=date_created&criteria=desc",
            Settings.AccessToken, null, null, ct);
        var root = json.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array ||
            !root.TryGetProperty("paging", out var paging) ||
            !paging.TryGetProperty("total", out var total) || !total.TryGetInt32(out var count) ||
            count < 0 || count > 100 || count != results.GetArrayLength())
            throw new SecurityFailure(409, "Billing.PaymentSearchRequiresReview", "A busca de pagamentos precisa de revisão.");

        var payments = results.EnumerateArray().Select(ReadPayment).ToList();

        if (payments.Any(Payment => Payment.ExternalReference != externalReference) ||
            payments.Select(Payment => Payment.Id).Distinct().Count() != payments.Count)
            throw Mismatch();

        return payments;
    }

    private async Task AccountAsync(CancellationToken ct)
    {
        Settings.RequireAccount();
        var account = await api.AccountAsync(Settings.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, Settings.Environment);

        if (account.Id != Settings.AccountId)
            throw new SecurityFailure(409, "Billing.AccountMismatch", "A credencial não pertence à conta recebedora da plataforma.");
    }

    private void ValidateCharge(PlatformBillingCharge charge, bool creating)
    {
        Settings.RequireAccount();
        ValidateReference(charge.ExternalReference);

        if (charge.AccountId != Settings.AccountId || charge.Environment != Settings.Environment)
            throw Mismatch();

        if (charge.Amount <= 0 || charge.Amount > 9999999999.99m || decimal.Round(charge.Amount, 2) != charge.Amount ||
            charge.Currency != "BRL" || charge.CheckoutExpiresAt.Kind != DateTimeKind.Utc ||
            creating && charge.CheckoutExpiresAt <= time.GetUtcNow().UtcDateTime)
            throw new SecurityFailure(400, "Billing.InvalidCharge", "A cobrança deve conter valor positivo em BRL e validade correta.");
    }

    private static void ValidateReference(string reference)
    {
        if (reference.Length != 41 || !reference.StartsWith("drp-saas-", StringComparison.Ordinal) ||
            !Guid.TryParseExact(reference[9..], "N", out _))
            throw new SecurityFailure(400, "Billing.InvalidReference", "Correlação da cobrança da plataforma inválida.");
    }

    private static PlatformBillingCheckout ReadCheckout(JsonElement root, PlatformBillingCharge charge)
    {
        if (MercadoPagoApi.Id(root, "collector_id") != charge.AccountId ||
            MercadoPagoApi.Text(root, "external_reference") != charge.ExternalReference ||
            !root.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array || items.GetArrayLength() != 1)
            throw Mismatch();

        var item = items[0];

        if (MercadoPagoApi.Text(item, "currency_id") != charge.Currency ||
            !item.TryGetProperty("quantity", out var quantity) || !quantity.TryGetInt32(out var count) || count != 1 ||
            MercadoPagoApi.Money(item, "unit_price") != charge.Amount)
            throw Mismatch();

        var checkoutUrl = MercadoPagoApi.Text(root, "init_point");

        if (!Uri.TryCreate(checkoutUrl, UriKind.Absolute, out var uri) || uri.Scheme != Uri.UriSchemeHttps ||
            !string.IsNullOrEmpty(uri.UserInfo) || !uri.IsDefaultPort ||
            uri.Host is not ("www.mercadopago.com.br" or "www.mercadopago.com" or "sandbox.mercadopago.com.br"))
            throw InvalidResponse();

        return new PlatformBillingCheckout(MercadoPagoApi.Id(root, "id"), checkoutUrl);
    }

    private PlatformBillingPayment ReadPayment(JsonElement root)
    {
        if (MercadoPagoApi.Id(root, "collector_id") != Settings.AccountId ||
            Settings.Environment == IntegrationEnvironment.Production &&
                (!root.TryGetProperty("live_mode", out var live) || live.ValueKind != JsonValueKind.True))
            throw Mismatch();

        if (!DateTimeOffset.TryParse(MercadoPagoApi.Text(root, "date_last_updated"), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var updatedAt))
            throw InvalidResponse();

        // Test users do Checkout Pro podem retornar live_mode=true. O ambiente foi
        // validado pela identidade test_user do recebedor, sem usar esse campo isoladamente.
        return new PlatformBillingPayment(MercadoPagoApi.Id(root, "id"), MercadoPagoApi.Text(root, "external_reference"),
            Settings.AccountId, MercadoPagoApi.Money(root, "transaction_amount"), MercadoPagoApi.Text(root, "currency_id"),
            MercadoPagoApi.Text(root, "status"), MercadoPagoApi.Money(root, "transaction_amount_refunded"), updatedAt.UtcDateTime);
    }

    private static SecurityFailure Mismatch()
    {
        return new SecurityFailure(409, "Billing.ScopeMismatch", "O resultado não corresponde ao recebedor e aos termos da plataforma.");
    }

    private static SecurityFailure InvalidResponse()
    {
        return new SecurityFailure(502, "Billing.InvalidResponse", "O provedor retornou uma resposta financeira inválida.");
    }
}
