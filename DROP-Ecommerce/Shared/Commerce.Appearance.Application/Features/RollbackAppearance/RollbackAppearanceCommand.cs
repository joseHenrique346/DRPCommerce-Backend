using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record RollbackAppearanceCommand(InputAppearanceTarget Target, long RevisionId, RollbackAppearanceRequest Request) : IRequest<AppearanceManifest>;
