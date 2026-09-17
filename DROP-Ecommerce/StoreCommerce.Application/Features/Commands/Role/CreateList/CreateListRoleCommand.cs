using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListRoleCommand(List<CreateRoleCommand> commands) : IRequest<Result<List<Role>>> { }
