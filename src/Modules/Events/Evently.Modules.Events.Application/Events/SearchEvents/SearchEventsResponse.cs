using Evently.Modules.Events.Application.Events.GetEvents;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

public sealed record SearchEventsResponse(
    int Skip,
    int Limit,
    int TotalCount,
    bool HasMore,
    IReadOnlyCollection<EventResponse> Events);
