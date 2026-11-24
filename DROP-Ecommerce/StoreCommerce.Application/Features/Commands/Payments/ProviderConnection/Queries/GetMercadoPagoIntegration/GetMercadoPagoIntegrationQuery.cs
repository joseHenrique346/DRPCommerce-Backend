using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetMercadoPagoIntegrationQuery() : IRequest<MercadoPagoIntegrationSummary>;
