using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Dados da cobrança guardados em Transaction.GatewayPayload para reexibir as instruções ao cliente.
/// </summary>
public sealed record PaymentSessionPayload(string? CheckoutUrl, string? Instructions, DateTime? ExpiresAt)
{
    public string Serialize() => JsonSerializer.Serialize(this);

    public static PaymentSessionPayload? Parse(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
            return null;

        try
        {
            return JsonSerializer.Deserialize<PaymentSessionPayload>(payload);
        }
        catch (JsonException)
        {
            return null;
        }
    }
}
