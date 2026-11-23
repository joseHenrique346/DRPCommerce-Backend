using StoreCommerce.Domain.Enums.Integrations;

namespace StoreCommerce.Application.Features.Commands;

public sealed record MercadoPagoIntegrationSummary(bool Enabled, bool ProductionAllowed, string ApplicationId, string SelectedProvider, IntegrationEnvironment? SelectedEnvironment, List<MercadoPagoConnectionSummary> Connections);
