using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllQueueSessionQueryHandler(IRepository<QueueSession> repository)
    : BaseGetAllHandler<GetAllQueueSessionQuery, QueueSession>(repository);
