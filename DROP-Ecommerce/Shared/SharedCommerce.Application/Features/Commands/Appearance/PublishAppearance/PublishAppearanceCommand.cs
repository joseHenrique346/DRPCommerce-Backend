using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Commands;

public sealed record PublishAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version) : IRequest<AppearanceManifest>;
