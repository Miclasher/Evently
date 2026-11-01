using Evently.Common.Infrastructure.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace Evently.Common.Infrastructure.FeatureFlags;

/// <summary>
/// Enables a feature only for users whose plan grants the configured permission.
/// </summary>
[FilterAlias("Plan")]
internal sealed class PlanFeatureFilter(IHttpContextAccessor httpContextAccessor) : IFeatureFilter
{
    public Task<bool> EvaluateAsync(FeatureFilterEvaluationContext context)
    {
        string? requiredPermission = context.Parameters["Permission"];

        if (string.IsNullOrWhiteSpace(requiredPermission) || httpContextAccessor.HttpContext?.User is null)
        {
            return Task.FromResult(false);
        }

        bool enabled = httpContextAccessor.HttpContext.User.GetPermissions().Contains(requiredPermission);

        return Task.FromResult(enabled);
    }
}
