using System.Text.Json;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.StaticEntity;
using OrderEntity = StoreCommerce.Domain.Entity.Order;

namespace StoreCommerce.Application.Contracts.Order;

/// <summary>
/// Finaliza a compra dos itens selecionados do carrinho. IdempotencyKey deve ser gerada pelo front a cada tentativa de compra
/// (ex.: um GUID): repetir a mesma chave devolve o mesmo pedido em vez de criar outro.
/// Endereço de entrega, em ordem de prioridade: AddressId (endereço salvo do cliente); UseProfileAddress (endereço padrão
/// salvo ou, se não houver, o do cadastro); ou os campos de endereço informados.
/// </summary>
public sealed record CheckoutRequest(string IdempotencyKey, long PaymentMethodId, string? CouponCode, bool UseProfileAddress,
    string? ShippingAddressLine, string? ShippingCity, long? ShippingStateId, string? ShippingZipCode, string? Notes,
    long? AddressId = null, string? ShippingQuoteToken = null, bool PickupAtStore = false);
