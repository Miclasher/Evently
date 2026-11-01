using Evently.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;

namespace Evently.Modules.Users.Presentation.Users;

internal sealed class GetFeatures : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/profile/features", async (IFeatureManager featureManager) =>
        {
            var features = new Dictionary<string, bool>();

            await foreach (string name in featureManager.GetFeatureNamesAsync())
            {
                features[name] = await featureManager.IsEnabledAsync(name);
            }

            return Results.Ok(features);
        })
        .RequireAuthorization(Permissions.GetUser)
        .WithTags(Tags.Users);
    }
}
