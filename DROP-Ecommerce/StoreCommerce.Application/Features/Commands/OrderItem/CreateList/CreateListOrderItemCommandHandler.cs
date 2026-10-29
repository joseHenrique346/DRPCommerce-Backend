using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Adiciona itens aos pedidos copiando nome, SKU e preço do catálogo e recalculando os valores do pedido.
/// Um produto/serviço já presente no pedido tem a quantidade somada ao item existente.
/// </summary>
public class CreateListOrderItemCommandHandler(
    IOrderRepository orderRepository,
    IProductRepository productRepository,
    IRepository<Service> serviceRepository,
    IUnitOfWork unitOfWork,
    StoreSettingsReader storeSettings,
    CategoryCatalogReader categoryReader)
    : IRequestHandler<CreateListOrderItemCommand, Result<List<OrderItem>>>
{
    public async Task<Result<List<OrderItem>>> Handle(CreateListOrderItemCommand request, CancellationToken cancellationToken)
    {
        if (!await storeSettings.IsStoreActiveAsync(cancellationToken))
            return Result<List<OrderItem>>.Failure(StoreSettingsReader.InactiveStoreError);

        var settings = await storeSettings.GetCurrentAsync(cancellationToken);
        var categoryResult = await categoryReader.ReadAsync(true, cancellationToken);

        if (categoryResult.IsFailure)
            return Result<List<OrderItem>>.Failure(categoryResult.Errors);

        var visibleCategories = categoryResult.Content!.Select(Category => Category.Id).ToHashSet();
        var commands = request.commands;
        var orders = (await orderRepository.GetListWithItemsAsync(commands.Select(command => command.orderId).Distinct().ToList(), cancellationToken))
            .ToDictionary(order => order.Id);
        var products = await LoadAsync(productRepository, commands.Select(command => command.productId), cancellationToken);
        var services = await LoadAsync(serviceRepository, commands.Select(command => command.serviceId), cancellationToken);
        var items = new List<OrderItem>(commands.Count);

        foreach (var command in commands)
        {
            if (!orders.TryGetValue(command.orderId, out var order))
                return Result<List<OrderItem>>.NotFound("Order.NotFound", $"Order com id {command.orderId} não encontrado.");

            if (!order.IsEditable)
                return Result<List<OrderItem>>.Validation("Order.NotEditable", $"O pedido {order.Id} só pode receber itens enquanto estiver pendente.");

            if (!settings.IsWithinItemLimit(order.TotalQuantity + command.quantity))
                return Result<List<OrderItem>>.Validation("Order.MaxItemsExceeded", $"O pedido {order.Id} excede o limite de {settings.MaxItemsPerOrder} itens da loja.");

            OrderItem item;

            if (command.productId is long productId)
            {
                if (!products.TryGetValue(productId, out var product))
                    return Result<List<OrderItem>>.NotFound("Product.NotFound", $"Product com id {productId} não encontrado.");

                if (!product.IsActive || !visibleCategories.Contains(product.CategoryId))
                    return Result<List<OrderItem>>.Validation("Product.Inactive", $"O produto {productId} está inativo.");

                var variant = command.productVariantId.HasValue
                    ? product.ListProductVariant.SingleOrDefault(Variant => Variant.Id == command.productVariantId)
                    : product.ListProductVariant.SingleOrDefault(Variant => Variant.IsDefault && !product.HasVariants);

                if (variant is null || !variant.IsActive || variant.IsDeleted)
                    return Result<List<OrderItem>>.Validation("ProductVariant.Required", "Selecione um SKU ativo pertencente ao produto.");

                item = order.AddProductItem(product, variant, command.quantity, settings);
            }
            else
            {
                var serviceId = command.serviceId!.Value;

                if (!services.TryGetValue(serviceId, out var service))
                    return Result<List<OrderItem>>.NotFound("Service.NotFound", $"Service com id {serviceId} não encontrado.");

                if (!service.IsActive)
                    return Result<List<OrderItem>>.Validation("Service.Inactive", $"O serviço {serviceId} está inativo.");

                item = order.AddServiceItem(service, command.quantity, settings);
            }

            if (!items.Contains(item))
                items.Add(item);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<List<OrderItem>>.Success(items);
    }

    private static async Task<Dictionary<long, TEntity>> LoadAsync<TEntity>(IRepository<TEntity> repository, IEnumerable<long?> ids, CancellationToken cancellationToken)
        where TEntity : BaseEntity
    {
        var listId = ids.Where(id => id.HasValue).Select(id => id!.Value).Distinct().ToList();

        if (listId.Count == 0)
            return [];

        return (await repository.GetListByListIdAsync(listId, cancellationToken)).ToDictionary(entity => entity.Id);
    }
}
