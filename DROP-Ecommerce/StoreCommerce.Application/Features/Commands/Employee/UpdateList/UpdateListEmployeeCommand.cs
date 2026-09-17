using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListEmployeeCommand(List<UpdateEmployeeCommand> commands) : IRequest<Result<List<Employee>>> { }
