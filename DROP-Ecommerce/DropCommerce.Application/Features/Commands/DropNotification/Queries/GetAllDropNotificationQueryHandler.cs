using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropNotificationQueryHandler(IRepository<DropNotification> repository)
    : BaseGetAllHandler<GetAllDropNotificationQuery, DropNotification>(repository);
