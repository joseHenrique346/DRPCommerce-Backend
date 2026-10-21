using Commerce.Appearance.Application;
using Commerce.Appearance.Application.Features;
using DropCommerce.Application.Features;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DropCommerce.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddDropApplication(this IServiceCollection services)
    {
        var assembly = typeof(DependencyInjection).Assembly;

        services.AddMediatR(cfg => cfg.RegisterServicesFromAssemblies(
            assembly,
            typeof(GetAppearanceStatusQueryHandler).Assembly));
        services.RemoveAll<IRequestHandler<ResolvePublishedAppearanceQuery, OutputResolvedAppearance?>>();
        services.AddTransient<IRequestHandler<ResolvePublishedAppearanceQuery, OutputResolvedAppearance?>,
            ResolveDropPublishedAppearanceQueryHandler>();
        services.AddValidatorsFromAssembly(assembly);
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));

        return services;
    }
}
