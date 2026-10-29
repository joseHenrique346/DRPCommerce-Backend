using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Exclui fisicamente cupons nunca vinculados a pedidos; os demais recebem exclusão lógica e continuam referenciados pelos pedidos.
/// </summary>
public class DeleteListCouponCommandHandler(
    IRepository<Coupon> repository,
    IRepository<Order> orderRepository,
    IRepository<CouponRedemption> redemptionRepository,
    IUnitOfWork unitOfWork)
    : BaseRelationalDeleteListHandler<DeleteListCouponCommand, Coupon>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListCouponCommand request) => request.ids;

    // Pedidos excluídos logicamente continuam com a chave estrangeira; por isso os filtros globais são ignorados.
    // O cupom já foi validado como pertencente à loja ao ser carregado.
    protected override async Task<bool> HasRelationsAsync(long id, CancellationToken cancellationToken) =>
        await orderRepository.ExistsAsync(order => order.CouponId == id, cancellationToken, ignoreQueryFilters: true) ||
        await redemptionRepository.ExistsAsync(redemption => redemption.CouponId == id, cancellationToken, ignoreQueryFilters: true);
}
