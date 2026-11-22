using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateEmployeePositionCommand(string name, string description, bool isActive) : IRequest<Result<EmployeePosition>>;
