using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListEnterpriseMembershipCommand(List<CreateEnterpriseMembershipCommand> commands) : IRequest<Result<List<EnterpriseMembership>>>;
