using MediatR;
using Microsoft.AspNetCore.Mvc;
using StoreCommerce.Api.Controllers.Base;
using StoreCommerce.Application.Contracts.Identity;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Api.Controllers;

[Route("api/users")]
public class UserController(IMediator mediator)
    : MappedCrudController<User, UserResponse, CreateUserCommand, CreateListUserCommand, UpdateUserCommand, UpdateListUserCommand>(mediator)
{
    protected override UserResponse MapEntity(User entity) => new(entity.Id, entity.FullName, entity.Email.Value, entity.NormalizedEmail, entity.EmailConfirmed, entity.IsActive, entity.LastLoginAt);
    protected override CreateListUserCommand WrapCreateInRange(CreateUserCommand command) => new([command]);
    protected override UpdateListUserCommand WrapUpdateInRange(UpdateUserCommand command) => new([command]);
    protected override IRequest<Result<bool>> DeleteRangeCommand(List<long> ids) => new DeleteListUserCommand(ids);
    protected override IRequest<Result<List<User>>> GetAllQuery() => new GetAllUserQuery();
    protected override IRequest<Result<User>> GetByIdQuery(long id) => new GetByIdUserQuery(id);
    protected override IRequest<Result<List<User>>> GetListByListIdQuery(List<long> ids) => new GetListByListIdUserQuery(ids);
}
