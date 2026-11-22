namespace DropCommerce.Domain.Entity;

public class DropAuditLog : BaseEntity
{
    #region Properties

    public long DropEventId { get; private set; }
    public long? CustomerId { get; private set; }
    public long? EmployeeId { get; private set; }
    public string Action { get; private set; }
    public string EntityName { get; private set; }
    public long EntityId { get; private set; }
    public string? OldValues { get; private set; }
    public string? NewValues { get; private set; }
    public string IpAddress { get; private set; }
    public string UserAgent { get; private set; }
    public DateTime OccurredAt { get; private set; }

    #region Navigation Properties

    public DropEvent DropEvent { get; private set; }

    #endregion

    #endregion

    #region Constructors

    protected DropAuditLog() { }

    private DropAuditLog(long dropEventId, long? customerId, long? employeeId, string action, string entityName, long entityId, string? oldValues, string? newValues, string ipAddress, string userAgent, DateTime occurredAt)
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

    #endregion

    #region Functions

    public static DropAuditLog Create(long dropEventId, long? customerId, long? employeeId, string action, string entityName, long entityId, string? oldValues, string? newValues, string ipAddress, string userAgent, DateTime occurredAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateIdNullable(customerId, nameof(customerId));
        BaseValidate.ValidateIdNullable(employeeId, nameof(employeeId));
        BaseValidate.ValidateString(action, nameof(action));
        BaseValidate.ValidateString(entityName, nameof(entityName));
        BaseValidate.ValidateId(entityId, nameof(entityId));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(userAgent, nameof(userAgent));
        BaseValidate.ValidateDate(occurredAt, nameof(occurredAt));

        return new DropAuditLog(dropEventId, customerId, employeeId, action, entityName, entityId, oldValues, newValues, ipAddress, userAgent, occurredAt);
    }

    public void Update(long dropEventId, long? customerId, long? employeeId, string action, string entityName, long entityId, string? oldValues, string? newValues, string ipAddress, string userAgent, DateTime occurredAt)
    {
        BaseValidate.ValidateId(dropEventId, nameof(dropEventId));
        BaseValidate.ValidateIdNullable(customerId, nameof(customerId));
        BaseValidate.ValidateIdNullable(employeeId, nameof(employeeId));
        BaseValidate.ValidateString(action, nameof(action));
        BaseValidate.ValidateString(entityName, nameof(entityName));
        BaseValidate.ValidateId(entityId, nameof(entityId));
        BaseValidate.ValidateRegexString(ipAddress, @"^(\d{1,3}\.){3}\d{1,3}$|^([0-9a-fA-F]{0,4}:){2,7}[0-9a-fA-F]{0,4}$", nameof(ipAddress));
        BaseValidate.ValidateString(userAgent, nameof(userAgent));
        BaseValidate.ValidateDate(occurredAt, nameof(occurredAt));

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

    #endregion
}
