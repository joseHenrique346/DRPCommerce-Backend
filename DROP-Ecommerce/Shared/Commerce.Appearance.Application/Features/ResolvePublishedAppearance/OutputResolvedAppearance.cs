using Commerce.Appearance.Domain;

namespace Commerce.Appearance.Application.Features;

public sealed record OutputResolvedAppearance(AppearanceRevision Revision, string Source);
