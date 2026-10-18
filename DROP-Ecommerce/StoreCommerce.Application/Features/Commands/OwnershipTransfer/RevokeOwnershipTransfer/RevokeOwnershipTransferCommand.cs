using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RevokeOwnershipTransferCommand(long Id) : IIdentityRequest<bool>;
