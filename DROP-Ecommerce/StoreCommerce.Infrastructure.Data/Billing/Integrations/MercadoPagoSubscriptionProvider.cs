using StoreCommerce.Application.Interfaces.Billing;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Billing;
using StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

namespace StoreCommerce.Infrastructure.Data.Billing;

public sealed class MercadoPagoSubscriptionProvider(MercadoPagoApi api, IOptions<PlatformBillingOptions> options)
    : IPlatformSubscriptionProvider
{
    private const int InvoicePageSize = 12;
    private PlatformBillingOptions Settings => options.Value;

    public async Task<PlatformRecurringSubscription> CreateAsync(string reference, string payerEmail, decimal monthlyAmount, CancellationToken ct)
    {
        Settings.RequireCheckout();
        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Post, "/preapproval", Settings.AccessToken, new
        {
            reason = "Plano DRPCommerce mensal",
            external_reference = reference,
            payer_email = payerEmail,
            auto_recurring = new { frequency = 1, frequency_type = "months", transaction_amount = monthlyAmount, currency_id = "BRL" },
            back_url = Settings.ReturnUrl,
            status = "pending"
        }, null, ct);
        var subscription = Read(json.RootElement);

        if (subscription.Reference != reference || subscription.MonthlyAmount != monthlyAmount)
            throw Mismatch();

        return subscription;
    }

    public async Task<PlatformRecurringSubscription?> RecoverAsync(string reference, CancellationToken ct)
    {
        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/preapproval/search?external_reference=" +
            Uri.EscapeDataString(reference) + "&limit=2", Settings.AccessToken, null, null, ct);
        var results = Results(json.RootElement);

        if (results.Count == 0)
            return null;

        if (results.Count != 1)
            throw Mismatch();

        var subscription = await GetAsync(MercadoPagoApi.Id(results[0], "id"), ct);

        if (subscription.Reference != reference)
            throw Mismatch();

        return subscription;
    }

    public async Task<PlatformRecurringSubscription> GetAsync(string id, CancellationToken ct)
    {
        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/preapproval/" + Uri.EscapeDataString(id),
            Settings.AccessToken, null, null, ct);
        var subscription = Read(json.RootElement);

        if (subscription.Id != id)
            throw Mismatch();

        return subscription;
    }

    public async Task<PlatformRecurringSubscription> UpdateAsync(string id, decimal monthlyAmount, bool cancel, CancellationToken ct)
    {
        await AccountAsync(ct);
        object body = cancel ? new { status = "canceled" } :
            new { auto_recurring = new { transaction_amount = monthlyAmount, currency_id = "BRL" } };
        using var json = await api.SendAsync(HttpMethod.Put, "/preapproval/" + Uri.EscapeDataString(id),
            Settings.AccessToken, body, null, ct);
        var subscription = Read(json.RootElement);

        if (subscription.Id != id || cancel && subscription.Status is not ("cancelled" or "canceled") ||
            !cancel && subscription.MonthlyAmount != monthlyAmount)
            throw Mismatch();

        return subscription;
    }

    public async Task<IReadOnlyList<PlatformRecurringInvoice>> InvoicesAsync(string subscriptionId, CancellationToken ct)
    {
        await AccountAsync(ct);
        var invoices = new List<PlatformRecurringInvoice>();

        // Consulta paginada. Uma busca truncada jamais é tratada como ausência de dívida.
        for (var offset = 0; offset < 10000; offset += InvoicePageSize)
        {
            using var json = await api.SendAsync(HttpMethod.Get, "/authorized_payments/search?preapproval_id=" +
                Uri.EscapeDataString(subscriptionId) + "&limit=" + InvoicePageSize.ToString(CultureInfo.InvariantCulture) +
                "&offset=" + offset.ToString(CultureInfo.InvariantCulture),
                Settings.AccessToken, null, null, ct);
            var root = json.RootElement;
            var results = root.GetProperty("results");
            var total = root.GetProperty("paging").GetProperty("total").GetInt32();

            if (total < offset || total > 10000 || results.ValueKind != JsonValueKind.Array ||
                results.GetArrayLength() != Math.Min(InvoicePageSize, total - offset))
                throw Mismatch();

            foreach (var item in results.EnumerateArray())
            {
                var invoice = ReadInvoice(item);

                if (invoice.SubscriptionId != subscriptionId || invoices.Any(Invoice => Invoice.Id == invoice.Id))
                    throw Mismatch();

                invoices.Add(invoice);
            }

            if (offset + results.GetArrayLength() >= total)
                return invoices;
        }

        throw Mismatch();
    }

    public async Task<PlatformRecurringInvoice> InvoiceAsync(string id, CancellationToken ct)
    {
        await AccountAsync(ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/authorized_payments/" + Uri.EscapeDataString(id),
            Settings.AccessToken, null, null, ct);
        var invoice = ReadInvoice(json.RootElement);

        if (invoice.Id != id)
            throw Mismatch();

        return invoice;
    }

    private async Task AccountAsync(CancellationToken ct)
    {
        Settings.RequireAccount();
        var account = await api.AccountAsync(Settings.AccessToken, ct);
        MercadoPagoApi.ValidateAccount(account, Settings.Environment);

        if (account.Id != Settings.AccountId)
            throw Mismatch();
    }

    private PlatformRecurringSubscription Read(JsonElement root)
    {
        var recurring = root.GetProperty("auto_recurring");

        if (MercadoPagoApi.Id(root, "collector_id") != Settings.AccountId ||
            recurring.GetProperty("frequency").GetInt32() != 1 ||
            MercadoPagoApi.Text(recurring, "frequency_type") != "months" ||
            MercadoPagoApi.Text(recurring, "currency_id") != "BRL")
            throw Mismatch();

        var checkout = MercadoPagoApi.Text(root, "init_point");

        if (checkout.Length > 0 && (!Uri.TryCreate(checkout, UriKind.Absolute, out var uri) ||
            uri.Scheme != "https" || !uri.IsDefaultPort || !string.IsNullOrEmpty(uri.UserInfo) ||
            uri.Host is not ("www.mercadopago.com.br" or "www.mercadopago.com" or "sandbox.mercadopago.com.br")))
            throw Mismatch();

        return new PlatformRecurringSubscription(MercadoPagoApi.Id(root, "id"),
            MercadoPagoApi.Text(root, "external_reference"), Settings.AccountId, Money(recurring, "transaction_amount"),
            "BRL", MercadoPagoApi.Text(root, "status"), checkout.Length == 0 ? null : checkout);
    }

    private static List<JsonElement> Results(JsonElement root)
    {
        var results = root.GetProperty("results");
        var total = root.GetProperty("paging").GetProperty("total").GetInt32();

        if (results.ValueKind != JsonValueKind.Array || total < 0 || total != results.GetArrayLength())
            throw Mismatch();

        return results.EnumerateArray().ToList();
    }

    private static PlatformRecurringInvoice ReadInvoice(JsonElement root)
    {
        if (!DateTimeOffset.TryParse(MercadoPagoApi.Text(root, "debit_date"), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var debit))
            throw Mismatch();

        string? paymentId = null;

        if (root.TryGetProperty("payment", out var payment) && payment.ValueKind == JsonValueKind.Object &&
            payment.TryGetProperty("id", out var id) && id.ValueKind is JsonValueKind.Number or JsonValueKind.String)
            paymentId = MercadoPagoApi.Id(payment, "id");

        return new PlatformRecurringInvoice(MercadoPagoApi.Id(root, "id"), MercadoPagoApi.Id(root, "preapproval_id"),
            Money(root, "transaction_amount"), MercadoPagoApi.Text(root, "currency_id"), debit.UtcDateTime, paymentId);
    }

    private static decimal Money(JsonElement root, string name)
    {
        var value = root.GetProperty(name);

        if (value.ValueKind == JsonValueKind.Number && value.TryGetDecimal(out var number) && number >= 0)
            return number;

        if (value.ValueKind == JsonValueKind.String && decimal.TryParse(value.GetString(),
            NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var text) && text >= 0)
            return text;

        throw Mismatch();
    }

    private static SecurityFailure Mismatch()
    {
        return new SecurityFailure(409, "Billing.SubscriptionMismatch", "A assinatura ou fatura externa exige revisão.");
    }
}
