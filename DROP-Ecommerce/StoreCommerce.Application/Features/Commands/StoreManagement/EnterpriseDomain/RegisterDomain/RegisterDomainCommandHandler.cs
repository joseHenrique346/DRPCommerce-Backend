using SharedCommerce.Domain.Exceptions;
using SharedCommerce.Application.Security;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterDomainCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterDomainCommand, DomainChallenge>(context)
{
    protected override async Task<DomainChallenge> ExecuteAsync(RegisterDomainCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var host = HostName.Normalize(request.Host);

        if (Uri.TryCreate(Options.StorefrontBaseUrl, UriKind.Absolute, out var root) &&
            string.Equals(host, root.IdnHost.TrimEnd('.'), StringComparison.OrdinalIgnoreCase))
            throw new SecurityFailure(400, "Domain.PlatformDomainReserved", "O domínio público da plataforma é reservado.");

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);

        var contract = await data.Enterprises.GetContractStateAsync(actor.EnterpriseId, ct) ?? throw NotFound();

        if (!StoreCommerce.Domain.Contracts.EnterpriseEntitlements.Resolve(contract.PlanId, contract.AddOnId).AllowsCustomDomain)
            throw new SecurityFailure(409, "Domain.PlanRequired", "O contrato não permite domínio próprio.");

        if (await data.EnterpriseDomains.ExistsAsync(x => x.NormalizedHost == host, ct, ignoreQueryFilters: true))
            throw new SecurityFailure(409, "Domain.InUse", "Domínio já cadastrado.");

        var raw = OpaqueToken.Create();
        var domain = EnterpriseDomain.Create(actor.EnterpriseId, host, false, false);
        domain.SetVerificationChallenge(OpaqueToken.Hash(raw));
        data.EnterpriseDomains.Add(domain);
        await unitOfWork.CommitAsync(ct);
        await tx.CommitAsync(ct);

        return new(domain.Id, host, "_drpcommerce." + host, "drpcommerce-verification=" + raw);
    }
}
