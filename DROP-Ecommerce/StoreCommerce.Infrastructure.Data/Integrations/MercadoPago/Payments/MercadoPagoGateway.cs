using StoreCommerce.Application.Integrations.Providers.Interfaces;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.Enums.Integrations;
using StoreCommerce.Domain.Enums.Payments;
using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoGateway(MercadoPagoApi api, MercadoPagoAccess access, IProviderConnectionStore connections,
    IOptions<MercadoPagoOptions> options, TimeProvider time) : IPaymentGateway
{
    public string Provider => IntegrationProviders.MercadoPago;
    private MercadoPagoOptions Settings => options.Value;

    public Task<PaymentProviderContext> GetContextAsync(long enterpriseId, CancellationToken cancellationToken)
    {
        return GetContextAsync(enterpriseId, IntegrationEnvironment.Sandbox, cancellationToken);
    }

    public async Task<PaymentProviderContext> GetContextAsync(long enterpriseId, IntegrationEnvironment environment, CancellationToken ct)
    {
        Settings.RequireCheckout(environment);
        var connection = await connections.GetAsync(enterpriseId, Provider, environment, ct);

        if (connection is not { IsConnected: true })
            throw new SecurityFailure(409, "MercadoPago.Disconnected", "Conecte a conta Mercado Pago desta loja e ambiente.");

        return new PaymentProviderContext(Provider, environment, connection.ExternalAccountId, connection.Id);
    }

    public async Task<PaymentSession> CreatePaymentAsync(PaymentRequest request, CancellationToken ct)
    {
        string token;

        try
        {
            Settings.RequireCheckout(request.Context.Environment);

            if (request.Amount <= 0 || request.CurrencyCode != "BRL" || request.ExpiresAt <= time.GetUtcNow().UtcDateTime)
                throw new ArgumentException("Pagamento Mercado Pago exige valor positivo em BRL e prazo válido.");

            token = await access.AccessAsync(request.EnterpriseId, request.Context, ct);
        }
        catch (Exception)
        {
            throw new PaymentNotSubmittedException();
        }
        var returnUrl = Settings.PaymentReturnUrl + (Settings.PaymentReturnUrl.Contains('?') ? "&" : "?") +
            (request.ReturnFlow == "drop" ? "dropOrderId=" : "orderId=") + request.OrderId.ToString(CultureInfo.InvariantCulture);
        var body = new
        {
            items = new[]
            {
                new { id = request.OrderId.ToString(CultureInfo.InvariantCulture), title = request.ReturnFlow == "drop" ? $"Pedido DROP {request.OrderId}" : $"Pedido {request.OrderId}",
                    quantity = 1, currency_id = request.CurrencyCode, unit_price = request.Amount }
            },
            payer = new { name = request.CustomerName, email = request.CustomerEmail },
            payment_methods = new
            {
                // O Checkout Pro não permite excluir saldo em conta.
                excluded_payment_types = new[] { "debit_card", "prepaid_card", "atm", "digital_currency" }
                    .Select(Type => new { id = Type }).ToArray()
            },
            external_reference = request.ExternalReference,
            notification_url = Settings.WebhookUrl(request.Context.ProviderConnectionId!.Value),
            back_urls = new { success = returnUrl, pending = returnUrl, failure = returnUrl },
            auto_return = "approved",
            statement_descriptor = Settings.StatementDescriptor,
            expires = true,
            expiration_date_from = time.GetUtcNow().UtcDateTime.AddMinutes(-1),
            expiration_date_to = request.ExpiresAt
        };
        // Preferências não têm a mesma garantia documentada de idempotência dos pagamentos/reembolsos.
        // A intenção durável envia um único POST; qualquer interrupção passa à busca por external_reference.
        using var response = await api.SendAsync(HttpMethod.Post, "/checkout/preferences", token, body, null, ct);

        return Session(response.RootElement, request);
    }

    public async Task<PaymentSession?> RecoverPaymentAsync(PaymentRequest request, CancellationToken ct)
    {
        Settings.RequireEnvironment(request.Context.Environment);
        var token = await access.AccessAsync(request.EnterpriseId, request.Context, ct);
        using var response = await api.SendAsync(HttpMethod.Get,
            "/checkout/preferences/search?external_reference=" + Uri.EscapeDataString(request.ExternalReference) + "&limit=2",
            token, null, null, ct);
        var root = response.RootElement;

        if (!root.TryGetProperty("elements", out var elements) || elements.ValueKind != JsonValueKind.Array)
            throw new SecurityFailure(502, "MercadoPago.InvalidSearch", "Busca de preferências inválida.");

        if (elements.GetArrayLength() == 0)
            return null;

        if (elements.GetArrayLength() != 1 || root.TryGetProperty("total", out var total) && total.GetInt32() != 1)
            throw new SecurityFailure(409, "MercadoPago.AmbiguousPreference", "Há mais de uma preferência para a mesma correlação.");

        var id = MercadoPagoApi.Id(elements[0], "id");
        using var preference = await api.SendAsync(HttpMethod.Get, "/checkout/preferences/" + Uri.EscapeDataString(id), token, null, null, ct);

        return Session(preference.RootElement, request);
    }

    private static PaymentSession Session(JsonElement preference, PaymentRequest request)
    {
        ValidatePreference(preference, request.Context, request.ExternalReference, request.Amount, request.CurrencyCode);
        var id = MercadoPagoApi.Id(preference, "id");
        // O Checkout Pro atual usa o init_point do vendedor de teste, com comprador também de teste.
        var checkoutUrl = MercadoPagoApi.Text(preference, "init_point");

        if (!Uri.TryCreate(checkoutUrl, UriKind.Absolute, out var url) || url.Scheme != "https" ||
            !string.IsNullOrEmpty(url.UserInfo) || !(url.Host == "www.mercadopago.com.br" ||
                url.Host == "www.mercadopago.com" || url.Host == "sandbox.mercadopago.com.br"))
            throw new SecurityFailure(502, "MercadoPago.InvalidCheckoutUrl", "O provedor retornou um endereço de checkout inválido.");

        return new PaymentSession(id, checkoutUrl, null, request.ExpiresAt, id);
    }

    private static void ValidatePreference(JsonElement preference, PaymentProviderContext context, string? externalReference,
        decimal amount, string? currency)
    {
        if (MercadoPagoApi.Id(preference, "collector_id") != context.ExternalAccountId ||
            MercadoPagoApi.Text(preference, "external_reference") != externalReference ||
            !preference.TryGetProperty("items", out var items) || items.ValueKind != JsonValueKind.Array)
            throw new SecurityFailure(409, "MercadoPago.ScopeMismatch", "A preferência não corresponde à conta e ao pedido originais.");

        decimal total = 0;

        foreach (var item in items.EnumerateArray())
        {
            if (MercadoPagoApi.Text(item, "currency_id") != currency || !item.GetProperty("quantity").TryGetInt32(out var quantity) || quantity <= 0)
                throw new SecurityFailure(409, "MercadoPago.AmountMismatch", "Moeda ou quantidade incompatível com o pedido.");

            total += MercadoPagoApi.Money(item, "unit_price") * quantity;
        }

        if (total != amount)
            throw new SecurityFailure(409, "MercadoPago.AmountMismatch", "A preferência tem valor diferente da intenção persistida.");
    }

    public async Task<MercadoPagoPayment> GetPaymentAsync(long enterpriseId, PaymentProviderContext context, string paymentId, CancellationToken ct)
    {
        Settings.RequireEnvironment(context.Environment);
        var token = await access.AccessAsync(enterpriseId, context, ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/v1/payments/" + Uri.EscapeDataString(paymentId), token, null, null, ct);
        var payment = ReadPayment(json.RootElement, context);

        if (payment.Id != paymentId)
            throw new SecurityFailure(409, "MercadoPago.PaymentMismatch", "O ID consultado não corresponde ao pagamento retornado.");

        return payment;
    }

    public async Task<IReadOnlyList<MercadoPagoPayment>> SearchPaymentsAsync(long enterpriseId, PaymentProviderContext context,
        string externalReference, CancellationToken ct)
    {
        var token = await access.AccessAsync(enterpriseId, context, ct);
        using var json = await api.SendAsync(HttpMethod.Get, "/v1/payments/search?external_reference=" +
            Uri.EscapeDataString(externalReference) + "&limit=100&sort=date_created&criteria=desc", token, null, null, ct);
        var root = json.RootElement;

        if (!root.TryGetProperty("results", out var results) || results.ValueKind != JsonValueKind.Array ||
            root.GetProperty("paging").GetProperty("total").GetInt32() > 100)
            throw new SecurityFailure(409, "MercadoPago.PaymentSearchRequiresReview", "A busca de pagamentos exige revisão.");

        var payments = results.EnumerateArray().Select(Payment => ReadPayment(Payment, context)).ToList();

        if (payments.Any(Payment => Payment.ExternalReference != externalReference))
            throw new SecurityFailure(409, "MercadoPago.ScopeMismatch", "A busca retornou pagamentos de outra correlação.");

        return payments;
    }

    private static MercadoPagoPayment ReadPayment(JsonElement payment, PaymentProviderContext context)
    {
        if (MercadoPagoApi.Id(payment, "collector_id") != context.ExternalAccountId ||
            context.Environment == IntegrationEnvironment.Production &&
                (!payment.TryGetProperty("live_mode", out var mode) || mode.ValueKind != JsonValueKind.True))
            throw new SecurityFailure(409, "MercadoPago.ScopeMismatch", "O pagamento não pertence ao recebedor e ambiente originais.");

        if (!DateTimeOffset.TryParse(MercadoPagoApi.Text(payment, "date_last_updated"), CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeUniversal, out var updatedAt))
            throw new SecurityFailure(502, "MercadoPago.InvalidPayment", "O pagamento não possui data de atualização válida.");

        return new MercadoPagoPayment(MercadoPagoApi.Id(payment, "id"), MercadoPagoApi.Text(payment, "external_reference"),
            context.ExternalAccountId, MercadoPagoApi.Money(payment, "transaction_amount"), MercadoPagoApi.Text(payment, "currency_id"),
            MercadoPagoApi.Text(payment, "status"), MercadoPagoApi.Money(payment, "transaction_amount_refunded"), updatedAt.UtcDateTime,
            MercadoPagoApi.Text(payment, "payment_method_id"), MercadoPagoApi.Text(payment, "payment_type_id"));
    }

    public async Task<PaymentOperationResult> ExecuteOperationAsync(PaymentOperationRequest request, CancellationToken ct)
    {
        if (request.Kind == PaymentOperationKind.Cancel)
            return await CancelAsync(request, false, ct);

        MercadoPagoPayment payment;
        string token;

        try
        {
            Settings.RequireEnvironment(request.Context.Environment);
            payment = await TargetPaymentAsync(request, ct);
            token = await access.AccessAsync(request.EnterpriseId, request.Context, ct);
        }
        catch (Exception)
        {
            throw new PaymentNotSubmittedException();
        }

        if (payment.Status == "refunded" && payment.RefundedAmount == request.Amount)
            return new PaymentOperationResult(PaymentOperationStatus.Succeeded, payment.Id);

        if (payment.Status != "approved" || payment.RefundedAmount != 0)
            return new PaymentOperationResult(PaymentOperationStatus.ReviewRequired);

        // O valor já foi validado contra o pagamento completo. O contrato do estorno integral exige corpo vazio.
        using var json = await api.SendAsync(HttpMethod.Post, "/v1/payments/" + Uri.EscapeDataString(payment.Id) + "/refunds",
            token, null, request.IdempotencyKey, ct);
        var refund = json.RootElement;

        if (MercadoPagoApi.Id(refund, "payment_id") != payment.Id || MercadoPagoApi.Money(refund, "amount") != request.Amount)
            return new PaymentOperationResult(PaymentOperationStatus.OutcomeUnknown);

        var status = MercadoPagoApi.Text(refund, "status");

        return status switch
        {
            "approved" => new PaymentOperationResult(PaymentOperationStatus.Succeeded, MercadoPagoApi.Id(refund, "id")),
            "rejected" => new PaymentOperationResult(PaymentOperationStatus.Failed),
            _ => new PaymentOperationResult(PaymentOperationStatus.Pending)
        };
    }

    public async Task<PaymentOperationResult> RecoverOperationAsync(PaymentOperationRequest request, CancellationToken ct)
    {
        if (request.Kind == PaymentOperationKind.Cancel)
            return await CancelAsync(request, true, ct);

        var payment = await TargetPaymentAsync(request, ct);

        if (payment.Status == "refunded" && payment.RefundedAmount == request.Amount)
            return new PaymentOperationResult(PaymentOperationStatus.Succeeded, payment.Id);

        return new PaymentOperationResult(payment.RefundedAmount > 0
            ? PaymentOperationStatus.ReviewRequired : PaymentOperationStatus.OutcomeUnknown);
    }

    private async Task<MercadoPagoPayment> TargetPaymentAsync(PaymentOperationRequest request, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PaymentId))
            throw new SecurityFailure(409, "MercadoPago.PaymentIdRequired", "Estorno exige um PaymentId verificado.");

        var payment = await GetPaymentAsync(request.EnterpriseId, request.Context, request.PaymentId, ct);
        ValidatePayment(payment, request.ExternalReference, request.Amount, request.CurrencyCode);

        return payment;
    }

    public static void ValidatePayment(MercadoPagoPayment payment, string? externalReference, decimal amount, string? currency)
    {
        if (payment.ExternalReference != externalReference || payment.Amount != amount || payment.CurrencyCode != currency)
            throw new SecurityFailure(409, "MercadoPago.PaymentMismatch", "Valor, moeda ou correlação do pagamento não correspondem ao pedido.");
    }

    private async Task<PaymentOperationResult> CancelAsync(PaymentOperationRequest request, bool recoverOnly, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.PreferenceId) || string.IsNullOrWhiteSpace(request.ExternalReference))
            return new PaymentOperationResult(PaymentOperationStatus.ReviewRequired);

        string token;

        try
        {
            Settings.RequireEnvironment(request.Context.Environment);
            token = await access.AccessAsync(request.EnterpriseId, request.Context, ct);
        }
        catch (Exception) when (!recoverOnly)
        {
            throw new PaymentNotSubmittedException();
        }

        var path = "/checkout/preferences/" + Uri.EscapeDataString(request.PreferenceId);
        using var json = await api.SendAsync(HttpMethod.Get, path, token, null, null, ct);
        ValidatePreference(json.RootElement, request.Context, request.ExternalReference, request.Amount, request.CurrencyCode);
        var expired = DateTimeOffset.TryParse(MercadoPagoApi.Text(json.RootElement, "expiration_date_to"),
            CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var expiresAt) &&
            expiresAt <= time.GetUtcNow() && json.RootElement.TryGetProperty("expires", out var flag) && flag.ValueKind == JsonValueKind.True;

        if (!recoverOnly && !expired)
        {
            using var updated = await api.SendAsync(HttpMethod.Put, path, token,
                new { expires = true, expiration_date_to = time.GetUtcNow().UtcDateTime }, null, ct);
            ValidatePreference(updated.RootElement, request.Context, request.ExternalReference, request.Amount, request.CurrencyCode);
            expired = DateTimeOffset.TryParse(MercadoPagoApi.Text(updated.RootElement, "expiration_date_to"),
                CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var updatedExpiry) && updatedExpiry <= time.GetUtcNow() &&
                updated.RootElement.TryGetProperty("expires", out var updatedFlag) && updatedFlag.ValueKind == JsonValueKind.True;
        }

        var payments = await SearchPaymentsAsync(request.EnterpriseId, request.Context, request.ExternalReference, ct);
        var pending = false;

        foreach (var payment in payments)
        {
            ValidatePayment(payment, request.ExternalReference, request.Amount, request.CurrencyCode);

            if (payment.Status == "approved")
                return new PaymentOperationResult(PaymentOperationStatus.Pending);

            if (payment.Status is "pending" or "authorized" or "in_process")
            {
                if (recoverOnly)
                {
                    pending = true;
                    continue;
                }

                using var cancelled = await api.SendAsync(HttpMethod.Put, "/v1/payments/" + Uri.EscapeDataString(payment.Id),
                    token, new { status = "cancelled" }, request.IdempotencyKey + "-" + payment.Id, ct);
                var result = ReadPayment(cancelled.RootElement, request.Context);
                ValidatePayment(result, request.ExternalReference, request.Amount, request.CurrencyCode);
                pending |= result.Id != payment.Id || result.Status != "cancelled";
            }
        }

        return expired && !pending ? new PaymentOperationResult(PaymentOperationStatus.Succeeded, request.PreferenceId)
            : new PaymentOperationResult(PaymentOperationStatus.Pending);
    }
}
