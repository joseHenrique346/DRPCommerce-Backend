namespace DropCommerce.Domain.Entity;

public class QueueSession : BaseEntity
{
    #region Properties

    public long QueueEntryId { get; private set; }
    public long CustomerId { get; private set; }
    public string Token { get; private set; }
    public long StatusId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastHeartbeatAt { get; private set; }

    #endregion

    #region Constructors

    protected QueueSession() { }

    private QueueSession(long queueEntryId, long customerId, string token, long statusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt)
    {
        QueueEntryId = queueEntryId;
        CustomerId = customerId;
        Token = token;
        StatusId = statusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
        LastHeartbeatAt = lastHeartbeatAt;
    }

    #endregion

    #region Functions

    public static QueueSession Create(long queueEntryId, long customerId, string token, long statusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt)
    {
        BaseValidate<long>.ValidateNotNullValue(queueEntryId);
        BaseValidate<long>.ValidateIdValue(queueEntryId);

        BaseValidate<long>.ValidateNotNullValue(customerId);
        BaseValidate<long>.ValidateIdValue(customerId);

        BaseValidate<string>.ValidateStringWhiteSpaceValue(token);

        BaseValidate<long>.ValidateNotNullValue(statusId);

        BaseValidate<long>.ValidateIdValue(statusId);

        BaseValidate<DateTime>.ValidateNotNullValue(issuedAt);
        BaseValidate<DateTime>.ValidateNotNullValue(expiresAt);
        BaseValidate<DateTime>.ValidateNotNullValue(lastHeartbeatAt);

        return new QueueSession(queueEntryId, customerId, token, statusId, issuedAt, expiresAt, lastHeartbeatAt);
    }

    #endregion
}