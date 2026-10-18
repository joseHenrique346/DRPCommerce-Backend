using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateAccessRoleCommand(long enterpriseId, string name, string description, bool isActive) : IRequest<Result<AccessRole>>;
