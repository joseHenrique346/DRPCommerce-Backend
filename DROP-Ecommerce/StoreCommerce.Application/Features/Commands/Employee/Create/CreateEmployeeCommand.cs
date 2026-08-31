using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateEmployeeCommand(long enterpriseId, string fullName, EmployeeEmail email, string passwordHash, Role roleId, Department departmentId, bool isActive, DateTime hiredAt) : IRequest<Result<Employee>> { }
