using System.ComponentModel;
using Evently.Common.Domain;
using Evently.Common.Infrastructure.Authentication;
using Evently.Modules.Events.Application.Categories.CreateCategory;
using Evently.Modules.Events.Application.Categories.GetCategories;
using Evently.Modules.Events.Application.Categories.GetCategory;
using Evently.Modules.Events.Application.Events.GetEvent;
using Evently.Modules.Events.Application.Events.SearchEvents;
using MediatR;
using ModelContextProtocol.Server;

namespace Evently.Api.Mcp;

[McpServerToolType]
internal sealed class EventlyMcpTools(ISender sender, IHttpContextAccessor httpContextAccessor)
{
    private const string ModifyCategoriesPermission = "categories:update";

    [McpServerTool(Name = "search_events", ReadOnly = true)]
    [Description("Searches published events. Supports filtering by category and start/end date, and paging with skip/limit.")]
    public async Task<object> SearchEventsAsync(
        [Description("Optional category id")] Guid? categoryId = null,
        [Description("Optional lower bound for the event start date (UTC)")] DateTime? startDate = null,
        [Description("Optional upper bound for the event end date (UTC)")] DateTime? endDate = null,
        [Description("Number of events to skip")] int skip = 0,
        [Description("Maximum number of events to return (1-100)")] int limit = 15)
    {
        Result<SearchEventsResponse> result = await sender.Send(
            new SearchEventsQuery(categoryId, startDate, endDate, skip, limit));

        return result.IsSuccess ? result.Value : ErrorOf(result);
    }

    [McpServerTool(Name = "get_event", ReadOnly = true)]
    [Description("Gets a single event with its ticket types.")]
    public async Task<object> GetEventAsync([Description("Event id")] Guid eventId)
    {
        Result<EventResponse> result = await sender.Send(new GetEventQuery(eventId));

        return result.IsSuccess ? result.Value : ErrorOf(result);
    }

    [McpServerTool(Name = "list_categories", ReadOnly = true)]
    [Description("Lists all event categories.")]
    public async Task<object> ListCategoriesAsync()
    {
        Result<IReadOnlyCollection<CategoryResponse>> result = await sender.Send(new GetCategoriesQuery());

        return result.IsSuccess ? result.Value : ErrorOf(result);
    }

    [McpServerTool(Name = "create_category", Destructive = false)]
    [Description("Creates a new event category. Requires the caller to have the categories:update permission.")]
    public async Task<object> CreateCategoryAsync([Description("Category name")] string name)
    {
        HttpContext? httpContext = httpContextAccessor.HttpContext;

        if (httpContext is null || !httpContext.User.GetPermissions().Contains(ModifyCategoriesPermission))
        {
            return new { Error = $"Forbidden: the '{ModifyCategoriesPermission}' permission is required." };
        }

        Result<Guid> result = await sender.Send(new CreateCategoryCommand(name));

        return result.IsSuccess ? new { CategoryId = result.Value } : ErrorOf(result);
    }

    private static object ErrorOf(Result result) => new { Error = result.Error.Code, result.Error.Description };
}
