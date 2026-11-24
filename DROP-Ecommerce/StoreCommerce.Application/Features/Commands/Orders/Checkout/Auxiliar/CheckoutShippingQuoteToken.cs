using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

internal sealed record CheckoutShippingQuoteToken(string Fingerprint, OrderShippingSnapshot Snapshot);
