using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record PreviewAppearanceCommand(InputAppearanceTarget Target, long SessionId, long Version) : IRequest<AppearancePreviewResponse>;
