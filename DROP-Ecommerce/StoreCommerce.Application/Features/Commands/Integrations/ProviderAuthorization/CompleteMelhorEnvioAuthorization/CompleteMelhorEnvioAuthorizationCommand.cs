using MediatR;
using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CompleteMelhorEnvioAuthorizationCommand(
    IntegrationEnvironment Environment,
    string State,
    string BrowserBinding,
    string? Code,
    string? Error) : IRequest;
