using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListEnterpriseMembershipCommand(List<UpdateEnterpriseMembershipCommand> commands) : IRequest<Result<List<EnterpriseMembership>>>;
