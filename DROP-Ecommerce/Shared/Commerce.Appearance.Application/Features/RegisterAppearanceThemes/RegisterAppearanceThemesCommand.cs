using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record RegisterAppearanceThemesCommand(InputThemeCatalog Input) : IRequest;
