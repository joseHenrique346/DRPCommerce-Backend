using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateEmployeeCommand(long id, string fullName, long positionId, long departmentId, bool isActive, DateTime hiredAt) : IRequest<Result<Employee>> { }
