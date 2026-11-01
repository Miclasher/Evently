using Evently.Common.Domain;
using Evently.Common.Presentation.Endpoints;
using Evently.Common.Presentation.Results;
using Evently.Modules.Events.Application.Events.SearchEvents;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.WebUtilities;

namespace Evently.Modules.Events.Presentation.Events;

internal sealed class SearchEvents : IEndpoint
{
    public void MapEndpoint(IEndpointRouteBuilder app)
    {
        app.MapGet("events/search", async (
            ISender sender,
            HttpRequest httpRequest,
            Guid? categoryId,
            DateTime? startDate,
            DateTime? endDate,
            int skip = 0,
            int limit = 15) =>
        {
            Result<SearchEventsResponse> result = await sender.Send(
                new SearchEventsQuery(categoryId, startDate, endDate, skip, limit));

            return result.Match(
                response => Results.Ok(new
                {
                    response.Skip,
                    response.Limit,
                    response.TotalCount,
                    NextLink = response.HasMore ? BuildNextLink(httpRequest, skip + limit) : null,
                    response.Events
                }),
                ApiResults.Problem);
        })
        .RequireAuthorization(Permissions.SearchEvents)
        .WithTags(Tags.Events);
    }

    private static string BuildNextLink(HttpRequest request, int nextSkip)
    {
        var query = request.Query
            .Where(q => !string.Equals(q.Key, "skip", StringComparison.OrdinalIgnoreCase))
            .ToDictionary(q => q.Key, string? (q) => q.Value.ToString(), StringComparer.OrdinalIgnoreCase);

        query["skip"] = nextSkip.ToString(System.Globalization.CultureInfo.InvariantCulture);

        return QueryHelpers.AddQueryString($"{request.Scheme}://{request.Host}{request.Path}", query);
    }
}
