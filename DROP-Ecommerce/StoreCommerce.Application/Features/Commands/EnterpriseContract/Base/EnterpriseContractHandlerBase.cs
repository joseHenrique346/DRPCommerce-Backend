using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Contracts;

namespace StoreCommerce.Application.Features.Commands;

public abstract class EnterpriseContractHandlerBase<TRequest, TResponse>(IdentityHandlerContext context)
    : IdentityHandlerBase<TRequest, TResponse>(context)
    where TRequest : IIdentityRequest<TResponse>
{
    protected async Task<EnterpriseContractState> ReadCurrentContractAsync(CancellationToken ct)
    {
        var enterpriseId = actor.EnterpriseId;

        if (!await data.EnterpriseMemberships.ExistsAsync(Membership =>
            Membership.Id == actor.MembershipId && Membership.EnterpriseId == enterpriseId &&
            Membership.UserId == actor.UserId && Membership.IsActive && Membership.User.IsActive &&
            Membership.Enterprise.IsActive, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(403, "Contract.MembershipRequired", "Vínculo ativo com a loja necessário.");

        return await data.Enterprises.GetContractStateAsync(enterpriseId, ct) ?? throw NotFound();
    }
}
