using MediatR;
using StoreCommerce.Application.Integrations.MercadoPago;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ListDueMercadoPagoPaymentsQuery() : IRequest<List<MercadoPagoNotificationKey>>;
