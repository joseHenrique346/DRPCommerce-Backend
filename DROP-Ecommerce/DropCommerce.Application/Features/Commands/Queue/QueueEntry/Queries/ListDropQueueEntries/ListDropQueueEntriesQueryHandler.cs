using DropCommerce.Application.Interfaces.Queue;
using System.Security.Cryptography;
using System.Text;
using DropCommerce.Application.Integrations.Store.Interfaces;
using DropCommerce.Domain.Entity;
using Microsoft.Extensions.Options;
using SharedCommerce.Application.Security;
using SharedCommerce.Domain.Exceptions;
using MediatR;
using DropCommerce.Application.Contracts;
using SharedCommerce.Application.Result;
using DropCommerce.Application.Security;

namespace DropCommerce.Application.Features.Commands;

public sealed class ListDropQueueEntriesQueryHandler(
    IDropQueueRepository persistence)
    : IRequestHandler<ListDropQueueEntriesQuery, Result<IReadOnlyList<DropQueueEntryResponse>>>
{
    public async Task<Result<IReadOnlyList<DropQueueEntryResponse>>> Handle(ListDropQueueEntriesQuery command, CancellationToken ct)
    {
        return Result<IReadOnlyList<DropQueueEntryResponse>>.Success(await persistence.ListCustomerEntriesAsync(ct));
    }
}
