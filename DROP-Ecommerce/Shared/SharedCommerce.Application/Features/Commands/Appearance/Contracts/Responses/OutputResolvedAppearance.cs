using SharedCommerce.Domain.Entity;

namespace SharedCommerce.Application.Features.Commands;

public sealed record OutputResolvedAppearance(AppearanceRevision Revision, string Source);
