using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateRoleCommand(long id, string name, string description) : IRequest<Result<Role>> { }
