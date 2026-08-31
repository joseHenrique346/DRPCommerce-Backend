using DropCommerce.Application.Features.Commands.Base.Handlers;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.Interfaces;

namespace DropCommerce.Application.Features.Commands;

public class GetAllDropProductQueryHandler(IRepository<DropProduct> repository)
    : BaseGetAllHandler<GetAllDropProductQuery, DropProduct>(repository);
