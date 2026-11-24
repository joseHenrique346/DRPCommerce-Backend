namespace StoreCommerce.Application.Contracts.Identity;

public sealed record EmployeeResponse(long Id, long EnterpriseId, long EnterpriseMembershipId, string FullName, long PositionId, long DepartmentId, bool IsActive, DateTime HiredAt)
{
    public long EmployeeId => Id;
}
