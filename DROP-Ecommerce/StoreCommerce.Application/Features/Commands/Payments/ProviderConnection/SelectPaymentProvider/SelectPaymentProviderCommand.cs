using MediatR;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record SelectPaymentProviderCommand(string Provider, IntegrationEnvironment Environment) : IRequest;
