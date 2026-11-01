using Evently.Common.Presentation.Endpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.FeatureManagement;

namespace Evently.Modules.Users.Presentation.Users;

internal sealed class GetPremiumPerks : IEndpoint
{
    private const string PremiumPageFeature = "PremiumPage";

    private static readonly string[] Perks =
    [
        "Early access to ticket sales",
        "Priority support",
        "Exclusive events"
    ];

    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("users/premium", async (IFeatureManager featureManager) =>
        {
            if (!await featureManager.IsEnabledAsync(PremiumPageFeature))
            {
                return Results.Problem(
                    title: "Users.PremiumRequired",
                    detail: "This page is available on the Premium plan only.",
                    statusCode: StatusCodes.Status403Forbidden);
            }

            return Results.Ok(new
            {
                Plan = "Premium",
                Perks
            });
        })
        .RequireAuthorization(Permissions.GetUser)
        .WithTags(Tags.Users);
    }
}
