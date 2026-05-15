using DropCommerce.Application.Result;
using DropCommerce.Domain.Entity;
using MediatR;

namespace DropCommerce.Application.Features.Commands;

public record class GetByIdDropTransactionQuery(long id) : IRequest<Result<DropTransaction>> { }
