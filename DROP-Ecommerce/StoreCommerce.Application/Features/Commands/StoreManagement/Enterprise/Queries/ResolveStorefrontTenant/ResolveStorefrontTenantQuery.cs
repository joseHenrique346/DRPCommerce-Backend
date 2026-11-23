using MediatR;

namespace StoreCommerce.Application.Features.Commands;

public sealed record ResolveStorefrontTenantQuery(string Slug) : IRequest<long?>;
