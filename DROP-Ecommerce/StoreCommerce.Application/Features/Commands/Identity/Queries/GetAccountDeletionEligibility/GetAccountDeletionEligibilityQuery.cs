using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Identity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record GetAccountDeletionEligibilityQuery(bool Customer) : IIdentityRequest<AccountDeletionEligibility>;
