using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateUserCommand(long id, string fullName, string email, bool isActive) : IRequest<Result<User>>;
