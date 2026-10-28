using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

/// <summary>
/// Atualiza as configurações da loja atual (cria o registro se ainda não existir).
/// </summary>
public record class UpdateStoreSettingsCommand(string currencyCode, decimal minOrderValue, int? maxItemsPerOrder, bool allowBackorder, int pendingOrderExpirationMinutes) : IRequest<Result<StoreSettings>> { }
