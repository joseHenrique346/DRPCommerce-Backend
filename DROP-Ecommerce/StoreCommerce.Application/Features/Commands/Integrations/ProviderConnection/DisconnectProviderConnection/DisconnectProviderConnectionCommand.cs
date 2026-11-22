using StoreCommerce.Domain.Enums.Integrations;
using MediatR;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Integrations;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record DisconnectProviderConnectionCommand(string Provider, IntegrationEnvironment Environment) : IRequest<Result<bool>>;
