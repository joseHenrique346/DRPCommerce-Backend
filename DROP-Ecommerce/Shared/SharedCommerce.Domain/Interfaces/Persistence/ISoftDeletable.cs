namespace SharedCommerce.Domain.Interfaces.Persistence;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    void SoftDelete();
}
