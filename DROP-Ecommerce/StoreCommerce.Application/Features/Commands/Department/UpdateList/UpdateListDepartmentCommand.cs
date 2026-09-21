using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateListDepartmentCommand(List<UpdateDepartmentCommand> commands) : IRequest<Result<List<Department>>> { }
