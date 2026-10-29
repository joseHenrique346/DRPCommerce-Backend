using MediatR;
using StoreCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record CategoryOrderItem(long Id, int DisplayOrder);

public sealed record ReorderCategoriesCommand(List<CategoryOrderItem> Items) : IRequest<Result<bool>>;
