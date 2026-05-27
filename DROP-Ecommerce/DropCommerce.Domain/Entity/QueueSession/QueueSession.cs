using DropCommerce.Domain.StaticEntity;

namespace DropCommerce.Domain.Entity;

public class QueueSession : BaseEntity
{
    #region Properties

    public long QueueEntryId { get; private set; }
    public long CustomerId { get; private set; }
    public string Token { get; private set; }
    public long QueueSessionStatusId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastHeartbeatAt { get; private set; }

    #region Navigation Properties

    public QueueEntry QueueEntry { get; private set; }
    public QueueSessionStatus QueueSessionStatus { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected QueueSession() { }

    private QueueSession(long queueEntryId, long customerId, string token, long queueSessionStatusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt)
    {
        QueueEntryId = queueEntryId;
        CustomerId = customerId;
        Token = token;
        QueueSessionStatusId = queueSessionStatusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
        LastHeartbeatAt = lastHeartbeatAt;
    }

    #endregion

    #region Functions

    public static QueueSession Create(long queueEntryId, long customerId, string token, long queueSessionStatusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt)
    {
        BaseValidate.ValidateId(queueEntryId, nameof(queueEntryId));
        BaseValidate.ValidateId(customerId, nameof(customerId));
        BaseValidate.ValidateString(token, nameof(token));
        BaseValidate.ValidateId(queueSessionStatusId, nameof(queueSessionStatusId));
        BaseValidate.ValidateDate(issuedAt, nameof(issuedAt));
        BaseValidate.ValidateDate(expiresAt, nameof(expiresAt));
        BaseValidate.ValidateDateRange(issuedAt, expiresAt, nameof(issuedAt), nameof(expiresAt));
        BaseValidate.ValidateDate(lastHeartbeatAt, nameof(lastHeartbeatAt));

        return new QueueSession(queueEntryId, customerId, token, queueSessionStatusId, issuedAt, expiresAt, lastHeartbeatAt);
    }

    #endregion
}
