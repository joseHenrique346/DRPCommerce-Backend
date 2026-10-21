using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record GetAppearanceThemesQuery() : IRequest<IReadOnlyList<OutputThemeDescription>>;
