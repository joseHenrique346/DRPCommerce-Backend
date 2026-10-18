using StoreCommerce.Domain.Entity;
using Commerce.Security;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class RegisterDomainCommandHandler(IdentityHandlerContext context) : IdentityHandlerBase<RegisterDomainCommand, DomainChallenge>(context)
{
    protected override async Task<DomainChallenge> ExecuteAsync(RegisterDomainCommand command, CancellationToken ct)
    {
        var request = command.Request;

        var host = HostName.Normalize(request.Host);
        var platformDomain = HostName.Normalize(Options.StorefrontBaseDomain);

        if (host == platformDomain || host.EndsWith("." + platformDomain, StringComparison.Ordinal))
            throw new SecurityFailure(400, "Domain.PlatformDomainReserved", "Domínios da plataforma são gerenciados pelo cadastro de lojas.");

        using var operation = scope.Begin();
        await using var tx = await BeginOwnerOperationAsync(ct);

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
