using MediatR;
using SharedCommerce.Application.Contracts.Appearance;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Validation;

namespace SharedCommerce.Application.Features.Commands;

public sealed class GetAppearanceThemesQueryHandler(
    IAppearanceRepository repository,
    IAppearanceContext context) : IRequestHandler<GetAppearanceThemesQuery, IReadOnlyList<OutputThemeDescription>>
{
    public async Task<IReadOnlyList<OutputThemeDescription>> Handle(GetAppearanceThemesQuery request, CancellationToken ct)
    {
        context.RequireAdministrator();
        var releases = await repository.GetReleasesAsync(ct);

        return releases.Select(Release => new OutputThemeDescription(
            Release.Id,
            Release.Key,
            Release.Version,
            Release.Name,
            Release.RendererContract,
            Release.IsDeprecated,
            Release.MigrationDeadline,
            AppearanceSettingsValidator.Parse(Release.SchemaJson),
            AppearanceSettingsValidator.Parse(Release.DefaultsJson))).ToList();
    }
}
