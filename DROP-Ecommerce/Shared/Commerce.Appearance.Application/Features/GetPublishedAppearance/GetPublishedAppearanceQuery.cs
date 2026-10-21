using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record GetPublishedAppearanceQuery(InputAppearanceTarget Target, long? RevisionId = null) : IRequest<AppearanceManifest>;
