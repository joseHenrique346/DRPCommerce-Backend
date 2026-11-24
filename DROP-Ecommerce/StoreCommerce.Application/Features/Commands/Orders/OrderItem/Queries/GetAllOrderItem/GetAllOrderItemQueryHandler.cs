using StoreCommerce.Domain.Interfaces.Persistence;
using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public class GetAllOrderItemQueryHandler(IRepository<OrderItem> repository)
    : BaseGetAllHandler<GetAllOrderItemQuery, OrderItem>(repository);
