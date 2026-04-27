namespace DropCommerce.Domain.Entity;

public class DropAuditLog : BaseEntity
{
    public long DropEventId { get; private set; }
    public long? CustomerId { get; private set; }
    public long? EmployeeId { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public long EntityId { get; private set; }
    public string OldValues { get; private set; }
    public string NewValues { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime OccurredAt { get; private set; }

    public DropAuditLog() { }

    public DropAuditLog(long dropEventId, long? customerId, long? employeeId, string action, string entityName, long entityId, string oldValues, string newValues, string ipAddress, string userAgent, DateTime occurredAt)
    {
        DropEventId = dropEventId;
        CustomerId = customerId;
        EmployeeId = employeeId;
        Action = action;
        EntityName = entityName;
        EntityId = entityId;
        OldValues = oldValues;
        NewValues = newValues;
        IpAddress = ipAddress;
        UserAgent = userAgent;
        OccurredAt = occurredAt;
    }
}
