using MediatR;
using StoreCommerce.Application.Interfaces.Identity;
using StoreCommerce.Application.Interfaces.Payments;
using StoreCommerce.Domain.StaticEntity;

namespace StoreCommerce.Application.Features.Commands;

public sealed class SelectPaymentProviderCommandHandler(
    ICurrentIdentity actor,
    IPaymentGatewayRegistry gateways)
    : IRequestHandler<SelectPaymentProviderCommand>
{
    public async Task Handle(SelectPaymentProviderCommand command, CancellationToken ct)
    {
        actor.RequirePermission(PermissionCodes.SettingsManage);
        await gateways.SelectAsync(actor.EnterpriseId, command.Provider, command.Environment, ct);
    }
}
