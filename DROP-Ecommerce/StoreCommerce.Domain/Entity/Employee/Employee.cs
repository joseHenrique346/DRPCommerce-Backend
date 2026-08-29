using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Employee : BaseEntity, ITenantEntity
{
    #region Properties
    public long EnterpriseId { get; private set; }
    public string FullName { get; private set; }
    public EmployeeEmail Email { get; private set; }
    public string PasswordHash { get; private set; }
    public long RoleId { get; private set; }
    public long DepartmentId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime HiredAt { get; private set; }

    #region Navigation Properties
    public Enterprise Enterprise { get; private set; }
    public Role Role { get; private set; }
    public Department Department { get; private set; }
    #endregion

    #endregion

    #region Constructor
    protected Employee() { }

    private Employee(long enterpriseId, string fullName, EmployeeEmail email, string passwordHash, long roleId, long departmentId, bool isActive, DateTime hiredAt)
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
    #endregion

    #region Functions
    public static Employee Create(long enterpriseId, string fullName, EmployeeEmail email, string passwordHash, long roleId, long departmentId, bool isActive, DateTime hiredAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));
        BaseValidate.ValidatePositive(roleId, nameof(RoleId));
        BaseValidate.ValidatePositive(departmentId, nameof(DepartmentId));
        BaseValidate.ValidateNotFuture(hiredAt, nameof(HiredAt));

        return new Employee(enterpriseId, fullName, email, passwordHash, roleId, departmentId, isActive, hiredAt);
    }

    public void UpdatePersonalInfo(string fullName, EmployeeEmail email, long roleId, long departmentId)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidateNotNull(email, nameof(Email));
        BaseValidate.ValidatePositive(roleId, nameof(RoleId));
        BaseValidate.ValidatePositive(departmentId, nameof(DepartmentId));

        FullName = fullName;
        Email = email;
        RoleId = roleId;
        DepartmentId = departmentId;
    }

    public void UpdatePasswordHash(string passwordHash)
    {
        BaseValidate.ValidateNotNullOrEmpty(passwordHash, nameof(PasswordHash));
        BaseValidate.ValidateMaxLength(passwordHash, 500, nameof(PasswordHash));

        PasswordHash = passwordHash;
    }
    #endregion
}
