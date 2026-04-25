namespace DropCommerce.Domain.Entity;

public class QueueSession : BaseEntity
{
    public long QueueEntryId { get; private set; }
    public long CustomerId { get; private set; }
    public string Token { get; private set; }
    public long StatusId { get; private set; }
    public DateTime IssuedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public DateTime LastHeartbeatAt { get; private set; }

    public QueueSession() { }

    public QueueSession(long queueEntryId, long customerId, string token, long statusId, DateTime issuedAt, DateTime expiresAt, DateTime lastHeartbeatAt)
    {
        QueueEntryId = queueEntryId;
        CustomerId = customerId;
        Token = token;
        StatusId = statusId;
        IssuedAt = issuedAt;
        ExpiresAt = expiresAt;
        LastHeartbeatAt = lastHeartbeatAt;
    }
}
