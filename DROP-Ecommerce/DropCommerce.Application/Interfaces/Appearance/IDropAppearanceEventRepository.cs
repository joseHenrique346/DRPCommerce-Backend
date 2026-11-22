using DropCommerce.Application.Appearance;
using DropCommerce.Domain.Entity;

namespace DropCommerce.Application.Interfaces.Appearance;

public interface IDropAppearanceEventRepository
{
    Task<DropEvent?> GetAsync(long enterpriseId, long eventId, CancellationToken ct);
    Task<long?> GetPublicIdBySlugAsync(long enterpriseId, string slug, CancellationToken ct);
}
