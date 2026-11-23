using MediatR;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record BeginMercadoPagoAuthorizationCommand(IntegrationEnvironment Environment) : IRequest<ProviderAuthorizationStart>;
