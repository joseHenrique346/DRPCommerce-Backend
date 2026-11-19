using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class GetCurrentEnterpriseContractQueryHandler(IdentityHandlerContext context)
    : EnterpriseContractHandlerBase<GetCurrentEnterpriseContractQuery, CurrentEnterpriseContractResponse>(context)
{
    protected override async Task<CurrentEnterpriseContractResponse> ExecuteAsync(
        GetCurrentEnterpriseContractQuery command, CancellationToken ct)
    {
        return EnterpriseContractResponseMapper.Current(await ReadCurrentContractAsync(ct));
    }
}
