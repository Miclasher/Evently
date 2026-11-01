using System.Globalization;
using Evently.Common.Application.Caching;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

internal static class SearchEventsCache
{
    internal static readonly TimeSpan Expiration = TimeSpan.FromMinutes(5);

    private const string VersionKey = "events:search:version";

    internal static async Task<string> CreateKeyAsync(
        ICacheService cacheService,
        SearchEventsQuery query,
        CancellationToken cancellationToken)
    {
        string version = await cacheService.GetAsync<string>(VersionKey, cancellationToken) ?? "0";

        return string.Create(
            CultureInfo.InvariantCulture,
            $"events:search:{version}:{query.CategoryId}:{query.StartDate?.Date:o}:{query.EndDate?.Date:o}:{query.Skip}:{query.Limit}");
    }

    /// <summary>
    /// Invalidates every cached search result by bumping the key version (old entries expire on their own).
    /// </summary>
    internal static Task InvalidateAsync(ICacheService cacheService, CancellationToken cancellationToken) =>
        cacheService.SetAsync(VersionKey, Guid.NewGuid().ToString("N"), cancellationToken: cancellationToken);
}
