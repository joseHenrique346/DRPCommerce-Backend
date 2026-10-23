using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record RetryAppearanceMigrationCommand(InputAppearanceTarget Target) : IRequest;
