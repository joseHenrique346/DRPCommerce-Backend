using System.Globalization;
using System.Text.Json;
using Microsoft.Extensions.Options;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations.MercadoPago;

public sealed record MercadoPagoPayment(string Id, string ExternalReference, string CollectorId, decimal Amount,
    string CurrencyCode, string Status, decimal RefundedAmount, DateTime UpdatedAt, string MethodId, string MethodType);
