using System.Reflection;

namespace Commerce.Appearance.Domain.StaticEntity;

public abstract class BaseStaticEntity : IEquatable<BaseStaticEntity>
{
    public long Id { get; }
    public string Description { get; }

    protected BaseStaticEntity(long id, string description)
    {
        Id = id;
        Description = description;
    }

    public static IEnumerable<T> GetAll<T>() where T : BaseStaticEntity
    {
        return typeof(T)
            .GetFields(BindingFlags.Public | BindingFlags.Static | BindingFlags.DeclaredOnly)
            .Select(Field => Field.GetValue(null))
            .Cast<T>();
    }

    public static T FromId<T>(long id) where T : BaseStaticEntity
    {
        return GetAll<T>().SingleOrDefault(Value => Value.Id == id)
            ?? throw new AppearanceFailure(400, "Appearance.InvalidClassification", "Classificação de aparência inválida.");
    }

    public override string ToString()
    {
        return Description;
    }

    public override bool Equals(object? obj)
    {
        return obj is BaseStaticEntity other && Equals(other);
    }

    public bool Equals(BaseStaticEntity? other)
    {
        if (other is null)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        return GetType() == other.GetType() && Id == other.Id;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(GetType(), Id);
    }

    public static bool operator ==(BaseStaticEntity? left, BaseStaticEntity? right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(BaseStaticEntity? left, BaseStaticEntity? right)
    {
        return !Equals(left, right);
    }
}
