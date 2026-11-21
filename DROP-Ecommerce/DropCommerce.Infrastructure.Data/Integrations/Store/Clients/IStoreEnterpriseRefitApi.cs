using DropCommerce.Infrastructure.Data.Integrations.Store.Contracts;
using Refit;

namespace DropCommerce.Infrastructure.Data.Integrations.Store.Clients;

public interface IStoreEnterpriseRefitApi
{
    [Get("/api/internal/drop-sale-payments/customers/{customerId}/profile")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Drop.DropBuyerProfile>>> DropBuyerAsync(long customerId, CancellationToken ct);

    [Post("/api/internal/drop-sale-payments")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Drop.DropSalePaymentReceipt>>> StartDropPaymentAsync(
        [Body] SharedCommerce.Application.Contracts.Drop.StartDropSalePayment request, CancellationToken ct);

    [Get("/api/internal/drop-sale-payments/{operationId}")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Drop.DropSalePaymentReceipt>>> DropPaymentAsync(Guid operationId, CancellationToken ct);

    [Post("/api/internal/drop-sale-payments/{operationId}/ack")]
    Task<HttpResponseMessage> AcknowledgeDropPaymentAsync(Guid operationId, [Body] DropPaymentAcknowledgment request, CancellationToken ct);
    [Get("/api/internal/enterprises/{enterpriseId}/drop-contract")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Drop.DropCommercialRights>>> DropContractAsync(
        long enterpriseId, CancellationToken ct);

    [Get("/api/internal/enterprises/get-by-id/{enterpriseId}")]
    Task<ApiResponse<StoreApiResponse<StoreEnterpriseResponse>>> GetByIdAsync(long enterpriseId, CancellationToken cancellationToken);

    [Get("/api/internal/enterprises/{enterpriseId}/appearance-rights/drop")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Appearance.AppearanceCapabilities>>> AppearanceRightsAsync(
        long enterpriseId, CancellationToken ct);

    [Post("/api/internal/media-assets/{assetId}/retain-for-drop")]
    Task<ApiResponse<StoreApiResponse<SharedCommerce.Application.Contracts.Appearance.MediaAssetReference>>> RetainMediaAsync(
        long assetId, CancellationToken ct);
}

public sealed record DropPaymentAcknowledgment(long Revision, bool FulfillmentClosed, bool RequiresReview = false);
