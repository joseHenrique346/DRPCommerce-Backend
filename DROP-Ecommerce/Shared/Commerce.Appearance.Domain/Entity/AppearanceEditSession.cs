using Commerce.Appearance.Domain.Entity.Base;
using Commerce.Appearance.Domain.StaticEntity;

namespace Commerce.Appearance.Domain;

public sealed class AppearanceEditSession : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public long AppearanceId { get; private set; }
    public long ThemeReleaseId { get; private set; }
    public long EditorId { get; private set; }
    public long? BaseRevisionId { get; private set; }
    public long? RestoredFromRevisionId { get; private set; }
    public string SettingsJson { get; private set; } = "{}";
    public string? PreviewJson { get; private set; }
    public long AppearanceEditSessionStatusId { get; private set; } = StaticEntity.AppearanceEditSessionStatus.InEdit.Id;
    public AppearanceEditSessionStatus AppearanceEditSessionStatus { get; private set; } = null!;
    public bool IsOpen { get; private set; } = true;
    public long Version { get; private set; } = 1;
    public DateTime ExpiresAt { get; private set; }

    private AppearanceEditSession()
    {
    }

    public static AppearanceEditSession Begin(
        long enterpriseId,
        long appearanceId,
        long releaseId,
        long editorId,
        long? baseRevisionId,
        string settingsJson,
        DateTime now,
        long? restoredFromRevisionId = null)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(appearanceId, nameof(AppearanceId));
        BaseValidate.ValidatePositive(releaseId, nameof(ThemeReleaseId));
        BaseValidate.ValidatePositive(editorId, nameof(EditorId));
        BaseValidate.ValidateNullablePositive(baseRevisionId, nameof(BaseRevisionId));
        BaseValidate.ValidateNullablePositive(restoredFromRevisionId, nameof(RestoredFromRevisionId));
        BaseValidate.ValidateNotNullOrEmpty(settingsJson, nameof(SettingsJson));
        BaseValidate.ValidateDate(now, nameof(now));

        return new AppearanceEditSession
        {
            EnterpriseId = enterpriseId,
            AppearanceId = appearanceId,
            ThemeReleaseId = releaseId,
            EditorId = editorId,
            BaseRevisionId = baseRevisionId,
            RestoredFromRevisionId = restoredFromRevisionId,
            SettingsJson = settingsJson,
            ExpiresAt = now.AddHours(24)
        };
    }

    public void RequireEditable(long editorId, long expectedVersion, DateTime now)
    {
        if (!IsOpen || ExpiresAt <= now)
            throw new AppearanceFailure(409, "Appearance.SessionClosed", "A sessão de edição terminou ou expirou.");

        if (EditorId != editorId)
            throw new AppearanceFailure(403, "Appearance.OtherEditor", "A sessão pertence a outro editor.");

        if (Version != expectedVersion)
            throw new AppearanceFailure(412, "Appearance.VersionConflict", "A edição mudou. Recarregue antes de salvar.");
    }

    public void Save(string settingsJson, DateTime now)
    {
        BaseValidate.ValidateNotNullOrEmpty(settingsJson, nameof(SettingsJson));
        BaseValidate.ValidateDate(now, nameof(now));

        SettingsJson = settingsJson;
        PreviewJson = null;
        AppearanceEditSessionStatusId = StaticEntity.AppearanceEditSessionStatus.InEdit.Id;
        ExpiresAt = now.AddHours(24);
        Version++;
    }

    public void Preview(string manifestJson, DateTime now)
    {
        BaseValidate.ValidateNotNullOrEmpty(manifestJson, nameof(PreviewJson));
        BaseValidate.ValidateDate(now, nameof(now));

        PreviewJson = manifestJson;
        AppearanceEditSessionStatusId = StaticEntity.AppearanceEditSessionStatus.InPreview.Id;
        ExpiresAt = now.AddHours(24);
        Version++;
    }

    public void Close(AppearanceEditSessionStatus status)
    {
        if (status == StaticEntity.AppearanceEditSessionStatus.InEdit || status == StaticEntity.AppearanceEditSessionStatus.InPreview)
            throw new AppearanceFailure(400, "Appearance.InvalidSessionStatus", "Use um status de encerramento para fechar a edição.");

        AppearanceEditSessionStatusId = status.Id;
        IsOpen = false;
        PreviewJson = null;
        Version++;
    }
}
