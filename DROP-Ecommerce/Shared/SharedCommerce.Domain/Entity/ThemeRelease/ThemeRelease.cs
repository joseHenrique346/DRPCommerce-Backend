using SharedCommerce.Domain.Entity.Base;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Domain.Entity;

public sealed class ThemeRelease : BaseEntity
{
    #region Properties
    public long AppearanceChannelId { get; private set; }
    public string Key { get; private set; } = string.Empty;
    public string Version { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public int RendererContract { get; private set; }
    public string SchemaJson { get; private set; } = "{}";
    public string DefaultsJson { get; private set; } = "{}";
    public string Checksum { get; private set; } = string.Empty;
    public DateTime? MigrationDeadline { get; private set; }
    public long? ReplacementReleaseId { get; private set; }
    public string MigrationJson { get; private set; } = "{}";
    public bool IsDeprecated => MigrationDeadline.HasValue;

    #region Navigation Properties
    public AppearanceChannel AppearanceChannel { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    private ThemeRelease() { }
    #endregion

    #region Functions
    public static ThemeRelease Create(AppearanceChannel channel, string key, string version, string name, int rendererContract, string schemaJson, string defaultsJson, string checksum)
    {
        BaseValidate.ValidatePositive(channel.Id, nameof(AppearanceChannelId));
        BaseValidate.ValidateNotNullOrEmpty(key, nameof(Key));
        BaseValidate.ValidateMaxLength(key, 80, nameof(Key));
        BaseValidate.ValidateNotNullOrEmpty(version, nameof(Version));
        BaseValidate.ValidateMaxLength(version, 40, nameof(Version));
        BaseValidate.ValidateNotNullOrEmpty(name, nameof(Name));
        BaseValidate.ValidateMaxLength(name, 200, nameof(Name));
        BaseValidate.ValidatePositive(rendererContract, nameof(RendererContract));
        BaseValidate.ValidateNotNullOrEmpty(schemaJson, nameof(SchemaJson));
        BaseValidate.ValidateNotNullOrEmpty(defaultsJson, nameof(DefaultsJson));
        BaseValidate.ValidateNotNullOrEmpty(checksum, nameof(Checksum));
        BaseValidate.ValidateMaxLength(checksum, 80, nameof(Checksum));

        return new ThemeRelease
        {
            AppearanceChannelId = channel.Id,
            Key = key,
            Version = version,
            Name = name,
            RendererContract = rendererContract,
            SchemaJson = schemaJson,
            DefaultsJson = defaultsJson,
            Checksum = checksum
        };
    }

    public void Deprecate(ThemeRelease replacement, DateTime deadline, string migrationJson)
    {
        if (replacement.Id == Id || replacement.AppearanceChannelId != AppearanceChannelId || replacement.Key != Key ||
            System.Version.Parse(replacement.Version) <= System.Version.Parse(Version))
            throw new AppearanceFailure(400, "Appearance.InvalidMigration", "A substituição precisa ser uma versão posterior do mesmo tema.");

        ReplacementReleaseId = replacement.Id;
        MigrationDeadline = deadline;
        MigrationJson = migrationJson;
    }
    #endregion
}