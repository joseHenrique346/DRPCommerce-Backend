namespace StoreCommerce.Domain.Entity;

public class Employee : BaseEntity
{
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
    public EmployeeEmail Email { get; private set; }
    public string PasswordHash { get; private set; }
    public Role RoleId { get; private set; }
    public Department DepartmentId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime HiredAt { get; private set; }

    public Employee() { }

    public Employee(long enterpriseId, string fullName, EmployeeEmail email, string passwordHash, Role roleId, Department departmentId, bool isActive, DateTime hiredAt)
    {
        EnterpriseId = enterpriseId;
        FullName = fullName;
        Email = email;
        PasswordHash = passwordHash;
        RoleId = roleId;
        DepartmentId = departmentId;
        IsActive = isActive;
        HiredAt = hiredAt;
    }
}