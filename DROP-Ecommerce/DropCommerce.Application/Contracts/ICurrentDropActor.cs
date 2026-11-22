namespace DropCommerce.Application.Contracts;

public interface ICurrentDropActor
{
    long EnterpriseId { get; }
    long CustomerId { get; }
    bool IsAuthenticated { get; }
    bool IsUser { get; }
    bool HasMembership { get; }
    bool HasPermission(string permission);
    string? UserAgent { get; }
    string? IpAddress { get; }
}
