using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteDropRegistrationCommand(long id) : IRequest<Result<bool>> { }
