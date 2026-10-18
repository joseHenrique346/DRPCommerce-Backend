using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RequestOwnershipTransferCommand(OwnershipTransferRequest Request) : IIdentityRequest<OneTimeLink>;
