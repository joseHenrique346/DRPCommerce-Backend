using System.Text.Json;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Options;
using StoreCommerce.Application.Integrations;
using StoreCommerce.Domain.Entity;
using StoreCommerce.Domain.Integrations;

namespace StoreCommerce.Infrastructure.Data.Integrations;

public sealed class ProviderIntegrationOptions
{
    public string KeyRingPath { get; set; } = string.Empty;
    public string KeyEncryptionCertificateThumbprint { get; set; } = string.Empty;
}
