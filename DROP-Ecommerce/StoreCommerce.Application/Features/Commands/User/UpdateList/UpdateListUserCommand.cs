using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListUserCommand(List<UpdateUserCommand> commands) : IRequest<Result<List<User>>>;
