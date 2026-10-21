using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record SaveAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version, SaveAppearanceRequest Request) : IRequest<AppearanceEditResponse>;
