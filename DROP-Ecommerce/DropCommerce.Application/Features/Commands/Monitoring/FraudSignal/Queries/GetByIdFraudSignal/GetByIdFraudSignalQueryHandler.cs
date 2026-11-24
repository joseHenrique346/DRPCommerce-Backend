using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdFraudSignalQueryHandler(IRepository<FraudSignal> repository)
    : BaseGetByIdHandler<GetByIdFraudSignalQuery, FraudSignal>(repository)
{
    protected override long GetById(GetByIdFraudSignalQuery request) => request.id;
}
