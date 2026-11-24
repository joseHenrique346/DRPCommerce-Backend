using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateEnterpriseMembershipCommand(long id, long kindId, bool isActive, DateTime joinedAt) : IRequest<Result<EnterpriseMembership>>;
