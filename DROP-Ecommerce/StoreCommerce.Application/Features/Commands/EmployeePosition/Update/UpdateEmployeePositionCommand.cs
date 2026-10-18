using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateEmployeePositionCommand(long id, string name, string description, bool isActive) : IRequest<Result<EmployeePosition>>;
