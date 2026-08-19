using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetListByListIdFraudSignalQueryHandler(IRepository<FraudSignal> repository)
    : BaseGetListByListIdHandler<GetListByListIdFraudSignalQuery, FraudSignal>(repository)
{
    protected override IReadOnlyCollection<long> GetListByListId(GetListByListIdFraudSignalQuery request) => request.listId;
}
