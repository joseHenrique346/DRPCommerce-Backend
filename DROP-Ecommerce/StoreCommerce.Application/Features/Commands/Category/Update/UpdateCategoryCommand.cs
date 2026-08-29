using MediatR;
using StoreCommerce.Application.Result;
using StoreCommerce.Domain.Entity;

namespace StoreCommerce.Application.Features.Commands;

public record class UpdateCategoryCommand(long id, long enterpriseId, long? parentCategoryId, string name, string slug, string dscription, string imageUrl, int displayOrder, bool isActive) : IRequest<Result<Category>> { }
