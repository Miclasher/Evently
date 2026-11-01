using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

namespace Evently.Common.Infrastructure.FeatureFlags;

internal static class FeatureFlagsExtensions
{
    internal static IServiceCollection AddFeatureFlags(this IServiceCollection services)
    {
        services.AddHttpContextAccessor();

        services.AddFeatureManagement().AddFeatureFilter<PlanFeatureFilter>();

        return services;
    }
}
