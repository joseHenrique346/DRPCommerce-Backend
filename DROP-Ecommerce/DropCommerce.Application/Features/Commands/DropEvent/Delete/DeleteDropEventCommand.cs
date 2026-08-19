using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteDropEventCommand(long id) : IRequest<Result<bool>> { }
