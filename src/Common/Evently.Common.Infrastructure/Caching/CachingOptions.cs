using Microsoft.Extensions.Caching.Distributed;

namespace Evently.Common.Infrastructure.Caching;

public static class CachingOptions
{
    private static DistributedCacheEntryOptions DefaultExpiration => new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5),
    };

    public static DistributedCacheEntryOptions Create(TimeSpan? expiration)
    {
        return expiration.HasValue 
            ? new DistributedCacheEntryOptions {  AbsoluteExpirationRelativeToNow = expiration.Value }
            : DefaultExpiration;
    }
}
