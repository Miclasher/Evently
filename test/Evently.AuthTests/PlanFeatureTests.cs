using System.Security.Claims;
using Evently.Common.Infrastructure.Authentication;
using Evently.Common.Infrastructure.FeatureFlags;
using Evently.Modules.Users.Domain.Users;
using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.FeatureManagement;

namespace Evently.AuthTests;

public class PlanFeatureTests
{
    private static async Task<bool> EvaluateAsync(HttpContext? httpContext, string? permission = "premium:access")
    {
        IConfiguration parameters = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["Permission"] = permission })
            .Build();

        var filter = new PlanFeatureFilter(new HttpContextAccessor { HttpContext = httpContext });

        return await filter.EvaluateAsync(new FeatureFilterEvaluationContext { Parameters = parameters });
    }

    private static DefaultHttpContext ContextWith(params string[] permissions)
    {
        var identity = new ClaimsIdentity("test");

        foreach (string permission in permissions)
        {
            identity.AddClaim(new Claim(CustomClaims.Permission, permission));
        }

        return new DefaultHttpContext { User = new ClaimsPrincipal(identity) };
    }

    [Fact]
    public async Task Should_EnableFeature_ForPremiumUser()
    {
        (await EvaluateAsync(ContextWith("users:read", "premium:access"))).Should().BeTrue();
    }

    [Fact]
    public async Task Should_DisableFeature_ForMemberUser()
    {
        (await EvaluateAsync(ContextWith("users:read"))).Should().BeFalse();
    }

    [Fact]
    public async Task Should_DisableFeature_WhenNoHttpContext()
    {
        (await EvaluateAsync(null)).Should().BeFalse();
    }

    [Fact]
    public async Task Should_DisableFeature_WhenPermissionNotConfigured()
    {
        (await EvaluateAsync(ContextWith("premium:access"), permission: null)).Should().BeFalse();
    }

    [Fact]
    public void ChangePlan_ShouldReplaceMemberWithPremium()
    {
        var user = User.Create("a@b.com", "A", "B", "identity");

        user.ChangePlan(Role.Premium);

        user.Roles.Should().ContainSingle().Which.Name.Should().Be(Role.Premium.Name);
    }

    [Fact]
    public void ChangePlan_ShouldBeIdempotent()
    {
        var user = User.Create("a@b.com", "A", "B", "identity");

        user.ChangePlan(Role.Member);

        user.Roles.Should().ContainSingle().Which.Name.Should().Be(Role.Member.Name);
    }

    [Fact]
    public void ChangePlan_ShouldDowngradeToMember()
    {
        var user = User.Create("a@b.com", "A", "B", "identity");
        user.ChangePlan(Role.Premium);

        user.ChangePlan(Role.Member);

        user.Roles.Should().ContainSingle().Which.Name.Should().Be(Role.Member.Name);
    }
}
