using System.Text.Json;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Contracts.Store;

public sealed record StorefrontProductDetailResponse(StorefrontProductResponse Product, List<StorefrontVariantResponse> Variants);
