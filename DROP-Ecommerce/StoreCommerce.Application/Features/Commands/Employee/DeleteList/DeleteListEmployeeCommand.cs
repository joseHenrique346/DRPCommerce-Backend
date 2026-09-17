using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public record class DeleteListEmployeeCommand(List<long> ids) : IRequest<Result<bool>> { }
