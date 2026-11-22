using StoreCommerce.Domain.Enums.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Application.Features.Commands;
using MediatR;
using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Orders;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed class MercadoPagoSignature(IOptions<MercadoPagoOptions> options, TimeProvider time)
{
    public string Verify(string paymentId, string requestId, string signature, IntegrationEnvironment environment = IntegrationEnvironment.Sandbox)
    {
        if (paymentId.Length is < 1 or > 40 || paymentId.Any(Character => !char.IsAsciiDigit(Character)))
            throw new SecurityFailure(401, "MercadoPago.InvalidSignature", "Identificador do pagamento inválido.");

        return MercadoPagoWebhookSignature.Verify(paymentId, requestId, signature,
            options.Value.SignatureSecret(environment), options.Value.SignatureToleranceSeconds, time.GetUtcNow());
    }
}
