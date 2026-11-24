using DropCommerce.Domain.Interfaces.Persistence;
using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropOrderQueryHandler(IRepository<DropOrder> repository)
    : BaseGetAllHandler<GetAllDropOrderQuery, DropOrder>(repository);
