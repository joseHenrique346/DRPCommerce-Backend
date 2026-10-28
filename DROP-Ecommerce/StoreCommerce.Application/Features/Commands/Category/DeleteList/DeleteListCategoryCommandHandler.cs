using StoreCommerce.Application.Features.Commands.Base.Handlers;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Exclui fisicamente categorias sem subcategorias, produtos ou serviços; as demais recebem exclusão lógica.
/// </summary>
public class DeleteListCategoryCommandHandler(
    IRepository<Category> repository,
    IRepository<Product> productRepository,
    IRepository<Service> serviceRepository,
    IUnitOfWork unitOfWork)
    : BaseRelationalDeleteListHandler<DeleteListCategoryCommand, Category>(repository, unitOfWork)
{
    protected override IReadOnlyCollection<long> GetIdList(DeleteListCategoryCommand request) => request.ids;

    // Os filtros globais são ignorados porque registros excluídos logicamente ainda mantêm a chave estrangeira.
    // A categoria já foi validada como pertencente à loja ao ser carregada.
    protected override async Task<bool> HasRelationsAsync(long id, CancellationToken cancellationToken) =>
        await Repository.ExistsAsync(category => category.ParentCategoryId == id, cancellationToken, ignoreQueryFilters: true) ||
        await productRepository.ExistsAsync(product => product.CategoryId == id, cancellationToken, ignoreQueryFilters: true) ||
        await serviceRepository.ExistsAsync(service => service.CategoryId == id, cancellationToken, ignoreQueryFilters: true);
}
