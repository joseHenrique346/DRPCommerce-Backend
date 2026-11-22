using MediatR;
using SharedCommerce.Application.Result;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ReorderCategoriesCommand(List<CategoryOrderItem> Items) : IRequest<Result<bool>>;
