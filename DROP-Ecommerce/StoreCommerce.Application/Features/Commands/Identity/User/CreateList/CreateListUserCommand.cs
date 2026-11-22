using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListUserCommand(List<CreateUserCommand> commands) : IRequest<Result<List<User>>>;
