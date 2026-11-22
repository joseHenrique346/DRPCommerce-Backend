using StoreCommerce.Application.Orders;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Features.Commands;
using StoreCommerce.Application.Identity;
using StoreCommerce.Application.Payments;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Enums.Orders;

/// <summary>
/// Etapas sem dados extras. O envio tem fluxo próprio (ShipAsync), porque registra transportadora e rastreio.
/// </summary>
public enum OrderStep
{
    StartProcessing = 1,
    Deliver = 3
}
