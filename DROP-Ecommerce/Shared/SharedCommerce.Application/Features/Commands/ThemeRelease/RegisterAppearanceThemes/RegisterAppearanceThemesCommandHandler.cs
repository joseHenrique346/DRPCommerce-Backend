using System.Text.Json;
using MediatR;
using SharedCommerce.Application.Interfaces;
using SharedCommerce.Application.Validation;
using SharedCommerce.Domain.Entity;
using SharedCommerce.Domain.Exceptions;

namespace SharedCommerce.Application.Features.Commands;

public sealed class RegisterAppearanceThemesCommandHandler(
    IAppearanceRepository repository,
    IAppearanceContext context,
    ThemeDefinitionValidator validator) : IRequestHandler<RegisterAppearanceThemesCommand>
{
    public async Task Handle(RegisterAppearanceThemesCommand request, CancellationToken ct)
    {
        context.RequireCatalogPublisher();
        var definitions = request.Input.Definitions;

        if (definitions is null || definitions.Count is < 1 or > 50)
            throw new AppearanceFailure(400, "Appearance.InvalidCatalog", "Envie entre uma e cinquenta definições de tema.");

        foreach (var definition in definitions)
        {
            if (definition is null)
                throw new AppearanceFailure(400, "Appearance.InvalidPackage", "Definição de tema inválida.");

            validator.Validate(definition, repository.Channel.Code);
        }

        if (definitions.Select(Definition => (Definition.Key, Definition.Version)).Distinct().Count() != definitions.Count)
            throw new AppearanceFailure(400, "Appearance.DuplicateTheme", "Não repita a mesma chave e versão no catálogo.");

        await using var transaction = await repository.BeginTransactionAsync(-1, ct);

        foreach (var definition in definitions.OrderBy(Definition => Version.Parse(Definition.Version)))
        {
            var schema = AppearanceSettingsValidator.Canonicalize(definition.Schema);
            var defaults = AppearanceSettingsValidator.Canonicalize(definition.Defaults);
            var checksum = AppearanceSettingsValidator.Checksum(JsonSerializer.Serialize(new
            {
                definition.Channel,
                definition.Key,
                definition.Version,
                definition.Name,
                definition.RendererContract,
                schema,
                defaults
            }));
            var current = await repository.GetReleaseAsync(definition.Key, definition.Version, ct);

            if (current is not null && current.Checksum != checksum)
                throw new AppearanceFailure(409, "Appearance.PackageChanged", "Uma versão existente foi alterada sem mudar de versão.");

            if (current is null)
            {
                repository.Add(ThemeRelease.Create(repository.Channel, definition.Key, definition.Version,
                    definition.Name, definition.RendererContract, schema, defaults, checksum));
            }
        }

        await repository.SaveAsync(ct);

        foreach (var definition in definitions.Where(Definition => Definition.ReplacementVersion is not null))
        {
            var current = await repository.GetReleaseAsync(definition.Key, definition.Version, ct);
            var replacement = await repository.GetReleaseAsync(definition.Key, definition.ReplacementVersion!, ct)
                ?? throw new AppearanceFailure(400, "Appearance.MigrationTargetMissing", "O tema substituto ainda não foi registrado.");
            var migration = definition.Migration.ValueKind == JsonValueKind.Undefined
                ? "{}"
                : definition.Migration.GetRawText();

            current!.Deprecate(replacement, definition.MigrationDeadline!.Value, migration);
        }

        await repository.SaveAsync(ct);
        await transaction.CommitAsync(ct);
    }
}
