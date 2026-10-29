using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Exclui logicamente pedidos cancelados ou pendentes que ainda não foram para pagamento. Pedidos em andamento precisam ser
/// cancelados antes, para que estoque, cupom e pagamento sejam devolvidos.
/// </summary>
public class DeleteListOrderCommandHandler(IRepository<Order> repository, IUnitOfWork unitOfWork)
    : BaseDeleteListHandler<DeleteListOrderCommand, Order>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListOrderCommand request) => request.ids;

    public override async Task<Result<bool>> Handle(DeleteListOrderCommand request, CancellationToken cancellationToken)
    {
        var cancelled = OrderStatus.Cancelled.Id;
        var pending = OrderStatus.Pending.Id;

        if (await Repository.ExistsAsync(order => request.ids.Contains(order.Id) &&
                order.OrderStatusId != cancelled && !(order.OrderStatusId == pending && !order.IsLocked), cancellationToken))
            return Result<bool>.Conflict("Order.DeleteNotAllowed",
                "Somente pedidos cancelados ou pendentes que não foram para pagamento podem ser excluídos. Cancele o pedido antes.");

        return await base.Handle(request, cancellationToken);
    }
}
