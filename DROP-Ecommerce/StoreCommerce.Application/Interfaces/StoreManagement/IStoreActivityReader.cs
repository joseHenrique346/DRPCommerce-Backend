using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Contracts.Common;

namespace StoreCommerce.Application.Interfaces.StoreManagement;

public interface IStoreActivityReader
{
    Task<PagedResponse<StoreActivityResponse>> ReadAsync(
        long enterpriseId, StoreActivityQueryRequest request, CancellationToken cancellationToken);
}
