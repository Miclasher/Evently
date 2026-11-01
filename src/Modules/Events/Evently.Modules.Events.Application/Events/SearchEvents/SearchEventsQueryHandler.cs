using System.Data.Common;
using Dapper;
using Evently.Common.Application.Caching;
using Evently.Common.Application.Data;
using Evently.Common.Application.Messaging;
using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.GetEvents;
using Evently.Modules.Events.Domain.Events;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

internal sealed class SearchEventsQueryHandler(IDbConnectionFactory dbConnectionFactory, ICacheService cacheService)
    : IQueryHandler<SearchEventsQuery, SearchEventsResponse>
{

    public async Task<Result<SearchEventsResponse>> Handle(
        SearchEventsQuery request,
        CancellationToken cancellationToken)
    {
        string cacheKey = await SearchEventsCache.CreateKeyAsync(cacheService, request, cancellationToken);

        SearchEventsResponse? cached = await cacheService.GetAsync<SearchEventsResponse>(cacheKey, cancellationToken);

        if (cached is not null)
        {
            return cached;
        }

        await using DbConnection connection = await dbConnectionFactory.OpenConnectionAsync();

        var parameters = new SearchEventsParameters(
            (int)EventStatus.Published,
            request.CategoryId,
            request.StartDate?.Date,
            request.EndDate?.Date,
            request.Limit,
            request.Skip);

        IReadOnlyCollection<EventResponse> events = await GetEventsAsync(connection, parameters);

        int totalCount = await CountEventsAsync(connection, parameters);

        var response = new SearchEventsResponse(
            request.Skip,
            request.Limit,
            totalCount,
            request.Skip + events.Count < totalCount,
            events);

        await cacheService.SetAsync(cacheKey, response, SearchEventsCache.Expiration, cancellationToken);

        return response;
    }

    private static async Task<IReadOnlyCollection<EventResponse>> GetEventsAsync(
        DbConnection connection,
        SearchEventsParameters parameters)
    {
        const string sql =
            $"""
             SELECT
                 id AS {nameof(EventResponse.Id)},
                 category_id AS {nameof(EventResponse.CategoryId)},
                 title AS {nameof(EventResponse.Title)},
                 description AS {nameof(EventResponse.Description)},
                 location AS {nameof(EventResponse.Location)},
                 starts_at_utc AS {nameof(EventResponse.StartsAtUtc)},
                 ends_at_utc AS {nameof(EventResponse.EndsAtUtc)}
             FROM events.events
             WHERE
                status = @Status AND
                (@CategoryId IS NULL OR category_id = @CategoryId) AND
                (@StartDate::timestamp IS NULL OR starts_at_utc >= @StartDate::timestamp) AND
                (@EndDate::timestamp IS NULL OR ends_at_utc >= @EndDate::timestamp)
             ORDER BY starts_at_utc
             OFFSET @Skip
             LIMIT @Take
             """;

        List<EventResponse> events = (await connection.QueryAsync<EventResponse>(sql, parameters)).AsList();

        return events;
    }

    private static async Task<int> CountEventsAsync(DbConnection connection, SearchEventsParameters parameters)
    {
        const string sql =
            """
            SELECT COUNT(*)
            FROM events.events
            WHERE
               status = @Status AND
               (@CategoryId IS NULL OR category_id = @CategoryId) AND
               (@StartDate::timestamp IS NULL OR starts_at_utc >= @StartDate::timestamp) AND
               (@EndDate::timestamp IS NULL OR ends_at_utc >= @EndDate::timestamp)
            """;

        int totalCount = await connection.ExecuteScalarAsync<int>(sql, parameters);

        return totalCount;
    }

    private sealed record SearchEventsParameters(
        int Status,
        Guid? CategoryId,
        DateTime? StartDate,
        DateTime? EndDate,
        int Take,
        int Skip);
}
