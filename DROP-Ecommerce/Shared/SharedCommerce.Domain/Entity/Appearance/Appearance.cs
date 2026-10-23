using SharedCommerce.Domain.Entity.Base;
using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Domain.StaticEntity;

namespace SharedCommerce.Domain.Entity;

public sealed class Appearance : BaseEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public long AppearanceTargetTypeId { get; private set; }
    public long? DropEventId { get; private set; }
    public long? PublishedRevisionId { get; private set; }
    public bool MigrationBlocked { get; private set; }
    public string? MigrationFailureCode { get; private set; }
    public long Version { get; private set; } = 1;

    #region Navigation Properties
    public AppearanceTargetType AppearanceTargetType { get; private set; } = null!;
    #endregion

    #endregion

    #region Constructors
    private Appearance() { }
    #endregion

    #region Functions
    public static Appearance Create(long enterpriseId, long appearanceTargetTypeId, long? dropEventId = null)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId), "Appearance.InvalidTenant", "Loja inválida.");
        var targetType = BaseStaticEntity.FromId<AppearanceTargetType>(appearanceTargetTypeId);

        if (targetType == AppearanceTargetType.DropSpecific && dropEventId is not > 0 ||
            targetType != AppearanceTargetType.DropSpecific && dropEventId.HasValue)
            throw new AppearanceFailure(400, "Appearance.InvalidTarget", "Informe um evento válido apenas para uma aparência de drop específico.");

        return new Appearance
        {
            EnterpriseId = enterpriseId,
            AppearanceTargetTypeId = targetType.Id,
            DropEventId = dropEventId
        };
    }

    public void Publish(long revisionId)
    {
        BaseValidate.ValidatePositive(revisionId, nameof(PublishedRevisionId));

        PublishedRevisionId = revisionId;
        MigrationBlocked = false;
        MigrationFailureCode = null;
        Version++;
    }

    public void BlockMigration(string code)
    {
        BaseValidate.ValidateNotNullOrEmpty(code, nameof(MigrationFailureCode));
        BaseValidate.ValidateMaxLength(code, 120, nameof(MigrationFailureCode));

        MigrationBlocked = true;
        MigrationFailureCode = code;
        Version++;
    }
    #endregion
}