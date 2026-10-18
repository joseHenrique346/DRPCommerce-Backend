using StoreCommerce.Domain.Entity.Base;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Domain.Entity;

public class Employee : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public long EnterpriseMembershipId { get; private set; }
    public string FullName { get; private set; } = string.Empty;
    public long PositionId { get; private set; }
    public long DepartmentId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime HiredAt { get; private set; }

    public Enterprise Enterprise { get; private set; } = null!;
    public EnterpriseMembership EnterpriseMembership { get; private set; } = null!;
    public EmployeePosition Position { get; private set; } = null!;
    public Department Department { get; private set; } = null!;

    protected Employee() { }

    private Employee(long enterpriseId, long enterpriseMembershipId, string fullName, long positionId, long departmentId, bool isActive, DateTime hiredAt)
    {
        BaseValidate.ValidatePositive(enterpriseId, nameof(EnterpriseId));
        BaseValidate.ValidatePositive(enterpriseMembershipId, nameof(EnterpriseMembershipId));
        EnterpriseId = enterpriseId;
        EnterpriseMembershipId = enterpriseMembershipId;
        UpdateDetails(fullName, positionId, departmentId, isActive, hiredAt);
    }

    public static Employee Create(long enterpriseId, long enterpriseMembershipId, string fullName, long positionId, long departmentId, bool isActive, DateTime hiredAt) =>
        new(enterpriseId, enterpriseMembershipId, fullName, positionId, departmentId, isActive, hiredAt);

    public void UpdateDetails(string fullName, long positionId, long departmentId, bool isActive, DateTime hiredAt)
    {
        BaseValidate.ValidateNotNullOrEmpty(fullName, nameof(FullName));
        BaseValidate.ValidateMaxLength(fullName, 255, nameof(FullName));
        BaseValidate.ValidatePositive(positionId, nameof(PositionId));
        BaseValidate.ValidatePositive(departmentId, nameof(DepartmentId));
        FullName = fullName;
        PositionId = positionId;
        DepartmentId = departmentId;
        IsActive = isActive;
        HiredAt = hiredAt;
    }
}
