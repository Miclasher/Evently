using FluentValidation;

namespace Evently.Modules.Events.Application.Events.SearchEvents;

internal sealed class SearchEventsQueryValidator : AbstractValidator<SearchEventsQuery>
{
    public SearchEventsQueryValidator()
    {
        RuleFor(q => q.Skip).GreaterThanOrEqualTo(0);
        RuleFor(q => q.Limit).InclusiveBetween(1, 100);
    }
}
