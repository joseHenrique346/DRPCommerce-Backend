using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdFraudSignalQueryHandler(IRepository<FraudSignal> repository)
    : BaseGetByIdHandler<GetByIdFraudSignalQuery, FraudSignal>(repository)
{
    protected override long GetById(GetByIdFraudSignalQuery request) => request.id;
}
