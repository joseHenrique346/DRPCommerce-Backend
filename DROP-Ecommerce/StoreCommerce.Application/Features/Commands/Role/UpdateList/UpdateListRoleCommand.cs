using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListRoleCommand(List<UpdateRoleCommand> commands) : IRequest<Result<List<Role>>> { }
