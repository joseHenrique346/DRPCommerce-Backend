using DropCommerce.Application.Integrations.Store.Contracts;

namespace DropCommerce.Application.Integrations.Store.Interfaces;

public interface IStoreEmployeeReader
{
    Task<StoreEmployeeData?> GetByIdAsync(long employeeId, long enterpriseId, CancellationToken cancellationToken);
}
