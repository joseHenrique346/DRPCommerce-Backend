using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropRegistrationCommand(List<long> ids) : IRequest<Result<bool>> { }
