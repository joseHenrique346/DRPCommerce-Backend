using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

public class DeleteListOrderCommandHandler(
    IRepository<Order> repository,
    IUnitOfWork unitOfWork,
    CouponEligibilityService eligibility,
    TimeProvider time)
    : BaseDeleteListHandler<DeleteListOrderCommand, Order>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListOrderCommand request) => request.ids;

    // Pedido excluído deixa de contar como compra: o uso do cupom volta a ficar disponível.
    protected override Task BeforeCommitAsync(IReadOnlyCollection<long> ids, CancellationToken cancellationToken) =>
        eligibility.ReleaseForOrdersAsync(ids, time.GetUtcNow().UtcDateTime, cancellationToken);
}
