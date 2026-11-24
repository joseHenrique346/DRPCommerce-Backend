using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Identity;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public sealed record RevokeSessionCommand(Guid FamilyId, bool Customer) : IIdentityRequest<bool>;
