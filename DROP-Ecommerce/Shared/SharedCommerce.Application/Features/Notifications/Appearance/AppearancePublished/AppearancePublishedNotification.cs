using MediatR;
using SharedCommerce.Application.Contracts.Appearance;

namespace SharedCommerce.Application.Features.Notifications;

public sealed record AppearancePublishedNotification(
    long EnterpriseId,
    InputAppearanceTarget Target,
    long RevisionId) : INotification;
