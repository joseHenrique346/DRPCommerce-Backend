using System.Text.Json.Serialization;
using StoreCommerce.Application.Contracts.Identity;

namespace StoreCommerce.Application.Identity;

public record DomainChallenge(long Id, string Host, string RecordName, string RecordValue);
