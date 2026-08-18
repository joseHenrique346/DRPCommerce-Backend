using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetByIdWaitlistEntryQueryHandler(IRepository<WaitlistEntry> repository)
    : BaseGetByIdHandler<GetByIdWaitlistEntryQuery, WaitlistEntry>(repository)
{
    protected override long GetById(GetByIdWaitlistEntryQuery request) => request.id;
}
