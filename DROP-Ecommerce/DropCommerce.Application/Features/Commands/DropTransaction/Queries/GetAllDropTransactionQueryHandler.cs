using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropTransactionQueryHandler(IRepository<DropTransaction> repository)
    : BaseGetAllHandler<GetAllDropTransactionQuery, DropTransaction>(repository);
