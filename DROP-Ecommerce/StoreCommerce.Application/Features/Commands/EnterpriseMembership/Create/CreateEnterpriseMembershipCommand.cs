using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateEnterpriseMembershipCommand(long userId, long kindId, bool isActive, DateTime joinedAt) : IRequest<Result<EnterpriseMembership>>;
