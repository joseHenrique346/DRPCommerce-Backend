using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CompleteMercadoPagoAuthorizationCommand(string State, string BrowserBinding, string? Code, string? Error) : IRequest;
