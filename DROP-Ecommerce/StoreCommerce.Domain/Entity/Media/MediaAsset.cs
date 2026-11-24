using StoreCommerce.Domain.Interfaces.Tenancy;
using System.Text.Json.Serialization;

namespace StoreCommerce.Domain.Entity;

public sealed class MediaAsset : BaseEntity, ITenantEntity
{
    public long EnterpriseId { get; private set; }
    public Guid PublicId { get; private set; }
    public string ContentHash { get; private set; } = string.Empty;
    public int Width { get; private set; }
    public int Height { get; private set; }
    public int ByteLength { get; private set; }
    public bool IsDeleted { get; private set; }
    public bool IsRetainedByDrop { get; private set; }

    [JsonIgnore]
    public byte[] Content { get; private set; } = [];

    private MediaAsset()
    {
    }

    public static MediaAsset Create(long tenant, byte[] content, string hash, int width, int height)
    {
        if (tenant <= 0 || content.Length is < 1 or > 1048576 || hash.Length != 64 || width <= 0 || height <= 0)
            throw new ArgumentException("Ativo de mídia inválido.");

        return new MediaAsset
        {
            EnterpriseId = tenant,
            PublicId = Guid.NewGuid(),
            Content = content.ToArray(),
            ContentHash = hash,
            Width = width,
            Height = height,
            ByteLength = content.Length
        };
    }

    public void Delete()
    {
        IsDeleted = true;
        Content = [];
        ByteLength = 0;
    }

    public void RetainForDrop()
    {
        IsRetainedByDrop = true;
    }
}
