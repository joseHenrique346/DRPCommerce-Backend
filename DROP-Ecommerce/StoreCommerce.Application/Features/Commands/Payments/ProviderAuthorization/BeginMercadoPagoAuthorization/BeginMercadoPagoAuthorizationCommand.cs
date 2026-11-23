using MediatR;
using StoreCommerce.Application.Integrations.MercadoPago;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record BeginMercadoPagoAuthorizationCommand(IntegrationEnvironment Environment) : IRequest<MercadoPagoAuthorizationStart>;
