using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Exclui fisicamente produtos que nunca foram vendidos; produtos presentes em pedidos recebem exclusão lógica.
/// </summary>
public class DeleteListProductCommandHandler(
    IRepository<Product> repository,
    IRepository<OrderItem> orderItemRepository,
    IUnitOfWork unitOfWork)
    : BaseRelationalDeleteListHandler<DeleteListProductCommand, Product>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListProductCommand request) => request.ids;

    // O filtro global de OrderItem oculta itens de pedidos excluídos logicamente, que ainda referenciam o produto.
    // O produto já foi validado como pertencente à loja ao ser carregado.
    protected override Task<bool> HasRelationsAsync(long id, CancellationToken cancellationToken) =>
        orderItemRepository.ExistsAsync(item => item.ProductId == id, cancellationToken, ignoreQueryFilters: true);
}
