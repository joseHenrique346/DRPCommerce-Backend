using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Interfaces;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Atualiza as configurações da loja. Lojas anteriores à criação desta funcionalidade ganham o registro na primeira atualização.
/// Pedidos já fora de Pendente mantêm os valores calculados; pedidos pendentes passam a usar as novas regras na próxima alteração.
/// </summary>
public class UpdateStoreSettingsCommandHandler(IRepository<StoreSettings> repository, IUnitOfWork unitOfWork, ITenantProvider tenantProvider)
    : IRequestHandler<UpdateStoreSettingsCommand, Result<StoreSettings>>
{
    public async Task<Result<StoreSettings>> Handle(UpdateStoreSettingsCommand request, CancellationToken cancellationToken)
    {
        var enterpriseId = tenantProvider.GetEnterpriseId();
        var settings = await repository.FindAsync(entity => entity.EnterpriseId == enterpriseId, cancellationToken);

        if (settings is null)
        {
            settings = StoreSettings.Create(enterpriseId, request.currencyCode, request.minOrderValue, request.maxItemsPerOrder, request.allowBackorder, request.pendingOrderExpirationMinutes);
            repository.Add(settings);
        }
        else
        {
            settings.UpdateDetails(request.currencyCode, request.minOrderValue, request.maxItemsPerOrder, request.allowBackorder, request.pendingOrderExpirationMinutes);
        }

        await unitOfWork.CommitAsync(cancellationToken);

        return Result<StoreSettings>.Success(settings);
    }
}
