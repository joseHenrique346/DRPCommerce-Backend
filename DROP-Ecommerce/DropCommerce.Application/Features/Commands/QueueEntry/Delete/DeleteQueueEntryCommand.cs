using DropCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteQueueEntryCommand(long id) : IRequest<Result<bool>> { }
