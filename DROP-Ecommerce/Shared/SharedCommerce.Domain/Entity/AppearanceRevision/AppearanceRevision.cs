using SharedCommerce.Domain.Entity.Base;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Domain.Entity;

public sealed class AppearanceRevision : BaseEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long AppearanceId { get; private set; }
    public long ThemeReleaseId { get; private set; }
    public long Number { get; private set; }
    public string SettingsJson { get; private set; } = "{}";
    public string Checksum { get; private set; } = string.Empty;
    public long PublishedBy { get; private set; }
    public DateTime PublishedAt { get; private set; }
    public long? RestoredFromRevisionId { get; private set; }
    public long AppearanceRevisionOriginId { get; private set; }

    #region Navigation Properties
    public AppearanceRevisionOrigin AppearanceRevisionOrigin { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    private AppearanceRevision() { }
    #endregion

    #region Functions
    public static AppearanceRevision Create(long enterpriseId, long appearanceId, long releaseId, long number, string settingsJson, string checksum, long editorId, DateTime now, long? restoredFromRevisionId = null, AppearanceRevisionOrigin? origin = null)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(appearanceId, nameof(AppearanceId));
        BaseValidate.ValidatePositive(releaseId, nameof(ThemeReleaseId));
        BaseValidate.ValidatePositive(number, nameof(Number));
        BaseValidate.ValidatePositiveOrZero(editorId, nameof(PublishedBy));
        BaseValidate.ValidateNullablePositive(restoredFromRevisionId, nameof(RestoredFromRevisionId));
        BaseValidate.ValidateNotNullOrEmpty(settingsJson, nameof(SettingsJson));
        BaseValidate.ValidateNotNullOrEmpty(checksum, nameof(Checksum));
        BaseValidate.ValidateMaxLength(checksum, 80, nameof(Checksum));
        BaseValidate.ValidateDate(now, nameof(PublishedAt));

        return new AppearanceRevision
        {
            EnterpriseId = enterpriseId,
            AppearanceId = appearanceId,
            ThemeReleaseId = releaseId,
            Number = number,
            SettingsJson = settingsJson,
            Checksum = checksum,
            PublishedBy = editorId,
            PublishedAt = now,
            RestoredFromRevisionId = restoredFromRevisionId,
            AppearanceRevisionOriginId = (origin ?? StaticEntity.AppearanceRevisionOrigin.Publish).Id
        };
    }
    #endregion
}