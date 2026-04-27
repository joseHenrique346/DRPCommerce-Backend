using System.Reflection;

namespace DropCommerce.Domain.StaticEntity;

public abstract class BaseStaticEntity : IEquatable<BaseStaticEntity>
{
    public long Id { get; }
    public string Description { get; }

    protected BaseStaticEntity(long id, string description)
    {
        Id = id;
        Description = description;
    }

    public static IEnumerable<T> GetAll<T>() where T : BaseStaticEntity =>
        typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(f => f.GetValue(null))
            .Cast<T>();

    public override string ToString() => Description;

    public override bool Equals(object? obj) => obj is BaseStaticEntity other && Equals(other);

    public bool Equals(BaseStaticEntity? other)
    {
        if (other is null) return false;
        if (ReferenceEquals(this, other)) return true;
        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode() => HashCode.Combine(GetType(), Id);

    public static bool operator ==(BaseStaticEntity? left, BaseStaticEntity? right) => Equals(left, right);
    public static bool operator !=(BaseStaticEntity? left, BaseStaticEntity? right) => !Equals(left, right);
}