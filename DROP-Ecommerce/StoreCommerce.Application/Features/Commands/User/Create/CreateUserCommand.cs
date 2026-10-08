using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateUserCommand(string fullName, string email, bool isActive) : IRequest<Result<User>>;
