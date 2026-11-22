using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetByIdEnterpriseQueryHandler(IRepository<Enterprise> repository)
    : BaseGetByIdHandler<GetByIdEnterpriseQuery, Enterprise>(repository)
{
    protected override long GetById(GetByIdEnterpriseQuery request) => request.id;
}
