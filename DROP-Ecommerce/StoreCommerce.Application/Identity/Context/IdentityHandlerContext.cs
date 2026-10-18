using Commerce.Security;

namespace StoreCommerce.Application.Identity;

public sealed class IdentityHandlerContext(
    IIdentityRepositories repositories,
    IIdentityUnitOfWork unitOfWork,
    ICurrentIdentity actor,
    IIdentityOperationScope scope,
    IAccountPasswordHasher passwords,
    ISessionTokenIssuer tokens,
    SecurityOptions options,
    TimeProvider time,
    IAccountActionDelivery delivery,
    IDomainOwnershipVerifier domainVerifier)
{
    public IIdentityRepositories Repositories { get; } = repositories;
    public IIdentityUnitOfWork UnitOfWork { get; } = unitOfWork;
    public ICurrentIdentity Actor { get; } = actor;
    public IIdentityOperationScope Scope { get; } = scope;
    public IAccountPasswordHasher Passwords { get; } = passwords;
    public ISessionTokenIssuer Tokens { get; } = tokens;
    public SecurityOptions Options { get; } = options;
    public TimeProvider Time { get; } = time;
    public IAccountActionDelivery Delivery { get; } = delivery;
    public IDomainOwnershipVerifier DomainVerifier { get; } = domainVerifier;
}
