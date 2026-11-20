using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SharedCommerce.Domain.Exceptions;
using StoreCommerce.Application.Contracts;
using StoreCommerce.Application.Contracts.Common;
using StoreCommerce.Infrastructure.Data.Context;

namespace StoreCommerce.Infrastructure.Data.Repositories;

public sealed class StoreActivityReader(StoreCommerceDbContext context) : IStoreActivityReader
{
    public async Task<PagedResponse<StoreActivityResponse>> ReadAsync(
        long enterpriseId, StoreActivityQueryRequest request, CancellationToken cancellationToken)
    {
        if (request.Page is < 1 or > 100000 || request.PageSize is < 1 or > 100 ||
            request.ProductId is <= 0 || request.EventType?.Length > 60 ||
            request.FromUtc.HasValue && request.FromUtc.Value.Kind != DateTimeKind.Utc ||
            request.ToUtc.HasValue && request.ToUtc.Value.Kind != DateTimeKind.Utc ||
            request.FromUtc > request.ToUtc)
            throw new SecurityFailure(400, "StoreActivity.InvalidFilter", "Filtros inválidos. Informe datas UTC com Z.");

        var query = context.StoreActivities.IgnoreQueryFilters().AsNoTracking()
            .Where(Activity => Activity.EnterpriseId == enterpriseId);

        if (request.ProductId.HasValue)
            query = query.Where(Activity => Activity.ProductId == request.ProductId);

        if (!string.IsNullOrWhiteSpace(request.EventType))
            query = query.Where(Activity => Activity.EventType == request.EventType);

        if (request.FromUtc.HasValue)
            query = query.Where(Activity => Activity.OccurredAt >= request.FromUtc);

        if (request.ToUtc.HasValue)
            query = query.Where(Activity => Activity.OccurredAt <= request.ToUtc);

        var count = await query.CountAsync(cancellationToken);
        var events = await query.OrderByDescending(Activity => Activity.Id)
            .Skip((request.Page - 1) * request.PageSize).Take(request.PageSize).ToListAsync(cancellationToken);
        var items = events.Select(Activity => new StoreActivityResponse(
            Activity.Id, Activity.ProductId, Activity.ProductVariantId, Activity.EventType, Activity.ProductName,
            Activity.SKU, Activity.ActorType, Activity.ActorUserId, Activity.ReasonCode, Activity.CorrelationId,
            Activity.OccurredAt, JsonSerializer.Deserialize<JsonElement>(Activity.DetailsJson))).ToList();

        return new PagedResponse<StoreActivityResponse>(items, count, request.Page, request.PageSize);
    }
}
