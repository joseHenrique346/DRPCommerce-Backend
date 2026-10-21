using Commerce.Appearance.Contracts;
using MediatR;

namespace Commerce.Appearance.Application.Features;

public sealed record AppearancePublishedNotification(
    long EnterpriseId,
    InputAppearanceTarget Target,
    long RevisionId) : INotification;
