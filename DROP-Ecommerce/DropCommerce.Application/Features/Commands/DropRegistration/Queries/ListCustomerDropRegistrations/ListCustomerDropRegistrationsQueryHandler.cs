using DropCommerce.Domain.Interfaces;
using DropCommerce.Application.Security;
using DropCommerce.Domain.Entity;
using DropCommerce.Domain.StaticEntity;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using DropCommerce.Application.Result;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListCustomerDropRegistrationsQueryHandler(
    IDropStorefrontRepository persistence)
    : IRequestHandler<ListCustomerDropRegistrationsQuery, Result<IReadOnlyList<DropCustomerRegistrationResponse>>>
{
    public async Task<Result<IReadOnlyList<DropCustomerRegistrationResponse>>> Handle(ListCustomerDropRegistrationsQuery command, CancellationToken ct)
    {
        return Result<IReadOnlyList<DropCustomerRegistrationResponse>>.Success(await persistence.ListRegistrationsAsync(ct));
    }
}
