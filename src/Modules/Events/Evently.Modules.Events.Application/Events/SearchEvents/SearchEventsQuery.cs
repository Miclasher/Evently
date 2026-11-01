using Evently.Common.Application.Messaging;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

public sealed record SearchEventsQuery(
    Guid? CategoryId,
    DateTime? StartDate,
    DateTime? EndDate,
    int Skip,
    int Limit) : IQuery<SearchEventsResponse>;
