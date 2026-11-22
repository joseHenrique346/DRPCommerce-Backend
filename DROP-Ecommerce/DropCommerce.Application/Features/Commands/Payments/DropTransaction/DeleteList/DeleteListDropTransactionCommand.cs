using SharedCommerce.Application.Result;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class DeleteListDropTransactionCommand(List<long> ids) : IRequest<Result<bool>> { }
