using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Security.Delivery;

/// <summary>
/// Mantém a entrega de ações de conta indisponível até existir um canal seguro de envio.
/// </summary>
public sealed class DisabledAccountActionDelivery : IAccountActionDelivery
{
    public bool IsAvailable => false;

    /// <summary>
    /// Recusa a entrega de tokens enquanto o envio não estiver configurado, evitando expor links de recuperação sem proteção.
    /// </summary>
    public Task DeliverAsync(string recipient, string token, AccountActionPurpose purpose, string publicBaseUrl, CancellationToken ct)
    {
        throw new SecurityFailure(503, "Identity.ActionDeliveryDisabled", "Entrega de tokens não configurada.");
    }
}
