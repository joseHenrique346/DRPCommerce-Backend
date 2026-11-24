namespace StoreCommerce.Application.Contracts;

public sealed record EnterpriseContractUsageResponse(
    long EnterpriseId,
    long ContractRevision,
    EnterpriseContractUsage Usage,
    int StoreProductLimit,
    long RemainingStoreProducts,
    long ExcessStoreProducts,
    int EmployeeLimit,
    long RemainingEmployeeSlots,
    long ExcessAdditionalMembers,
    int DropProductLimit);
