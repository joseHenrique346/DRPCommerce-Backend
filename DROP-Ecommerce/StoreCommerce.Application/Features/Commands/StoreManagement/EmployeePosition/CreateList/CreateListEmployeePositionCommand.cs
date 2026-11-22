using MediatR;
using SharedCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateListEmployeePositionCommand(List<CreateEmployeePositionCommand> commands) : IRequest<Result<List<EmployeePosition>>>;
