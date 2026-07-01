using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropOrderQueryHandler(IRepository<DropOrder> repository)
    : BaseGetAllHandler<GetAllDropOrderQuery, DropOrder>(repository);
