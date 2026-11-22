using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdFraudSignalQueryHandler(IRepository<FraudSignal> repository)
    : BaseGetListByListIdHandler<GetListByListIdFraudSignalQuery, FraudSignal>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdFraudSignalQuery request) => request.listId;
}
