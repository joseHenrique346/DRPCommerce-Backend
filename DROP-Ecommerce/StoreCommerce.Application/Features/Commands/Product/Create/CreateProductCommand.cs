using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity.Product;

namespace StoreCommerce.Application.Features.Commands;

public record class CreateProductCommand(long enterpriseId, long categoryId, long? supplierId, string name, string slug, string description, string sku, string barCode, decimal price, decimal costPrice, decimal weight, decimal height, decimal width, decimal length, string brand, string imageUrls, bool isActive, bool isDigital) : IRequest<Result<Product>> { }
