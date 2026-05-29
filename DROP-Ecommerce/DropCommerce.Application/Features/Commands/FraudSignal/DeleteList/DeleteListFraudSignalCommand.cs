using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListFraudSignalCommand(List<long> ids) : IRequest<Result<bool>> { }
