using System.Security.Claims;
using Evently.Common.Infrastructure.Authentication;
using Evently.Common.Infrastructure.Authorization;
using FluentAssertions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace Evently.AuthTests;

public class PermissionAuthorizationTests
{
    private static ClaimsPrincipal UserWith(params string[] permissions)
    {
        var identity = new ClaimsIdentity("test");

        foreach (string permission in permissions)
        {
            identity.AddClaim(new Claim(CustomClaims.Permission, permission));
        }

        return new ClaimsPrincipal(identity);
    }

    private static async Task<bool> AuthorizeAsync(ClaimsPrincipal user, string permission)
    {
        var requirement = new PermissionRequirement(permission);
        var context = new AuthorizationHandlerContext([requirement], user, null);

        await new PermissionAuthorizationHandler().HandleAsync(context);

        return context.HasSucceeded;
    }

    [Fact]
    public async Task Should_Succeed_WhenUserHasPermission()
    {
        bool result = await AuthorizeAsync(UserWith("events:search"), "events:search");

        result.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Fail_WhenUserLacksPermission()
    {
        bool result = await AuthorizeAsync(UserWith("users:read"), "events:update");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_WhenUserHasNoPermissions()
    {
        bool result = await AuthorizeAsync(UserWith(), "users:read");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task Should_NotLeakPermissions_BetweenSimilarCodes()
    {
        bool result = await AuthorizeAsync(UserWith("events:read"), "events:update");

        result.Should().BeFalse();
    }

    [Fact]
    public async Task PolicyProvider_ShouldBuildPermissionRequirement_ForUnknownPolicy()
    {
        var provider = new PermissionAuthorizationPolicyProvider(Options.Create(new AuthorizationOptions()));

        AuthorizationPolicy? policy = await provider.GetPolicyAsync("orders:create");

        policy.Should().NotBeNull();
        policy.Requirements.Should().ContainSingle()
            .Which.Should().BeOfType<PermissionRequirement>()
            .Which.Permission.Should().Be("orders:create");
    }

    [Fact]
    public void GetUserId_ShouldThrow_WhenSubClaimMissing()
    {
        Action act = () => UserWith("users:read").GetUserId();

        act.Should().Throw<Exception>();
    }

    [Fact]
    public void GetUserId_ShouldReturnId_WhenSubClaimPresent()
    {
        var id = Guid.NewGuid();
        ClaimsPrincipal user = UserWith();
        ((ClaimsIdentity)user.Identity!).AddClaim(new Claim(CustomClaims.Sub, id.ToString()));

        user.GetUserId().Should().Be(id);
    }
}
