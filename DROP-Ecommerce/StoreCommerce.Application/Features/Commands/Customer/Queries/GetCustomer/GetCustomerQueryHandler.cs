using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCustomerQueryHandler(IdentityHandlerContext context) : IdentityHandlerBase<GetCustomerQuery, CustomerResponse>(context)
{
    protected override async Task<CustomerResponse> ExecuteAsync(GetCustomerQuery command, CancellationToken ct)
    {
        return CustomerResponse.FromEntity((Customer)await CurrentAccountAsync(true, false, ct));
    }
}
