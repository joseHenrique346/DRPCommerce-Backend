using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListAccessRoleCommand(List<CreateAccessRoleCommand> commands) : IRequest<Result<List<AccessRole>>>;
