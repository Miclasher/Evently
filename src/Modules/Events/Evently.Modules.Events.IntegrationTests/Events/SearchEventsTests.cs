using Evently.Common.Domain;
using Evently.Modules.Events.Application.Events.PublishEvent;
using Evently.Modules.Events.Application.Events.SearchEvents;
using Evently.Modules.Events.IntegrationTests.Abstractions;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace Evently.Modules.Events.IntegrationTests.Events;

public class SearchEventsTests : BaseIntegrationTest
{
    public SearchEventsTests(IntegrationTestWebAppFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task Should_ReturnCachedResult_OnSecondSearch()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());
        Guid eventId = await Sender.CreateEventAsync(categoryId);
        await Sender.CreateTicketTypeAsync(eventId);
        await Sender.Send(new PublishEventCommand(eventId));

        var query = new SearchEventsQuery(categoryId, null, null, 0, 10);

        Result<SearchEventsResponse> first = await Sender.Send(query);

        // Remove the data behind the cache's back
        await DbContext.Database.ExecuteSqlRawAsync("DELETE FROM events.ticket_types; DELETE FROM events.events;");

        // Act
        Result<SearchEventsResponse> second = await Sender.Send(query);

        // Assert
        first.Value.Events.Should().HaveCount(1);
        second.Value.Events.Should().HaveCount(1);
        second.Value.TotalCount.Should().Be(first.Value.TotalCount);
    }

    [Fact]
    public async Task Should_ApplySkipAndLimit()
    {
        // Arrange
        await CleanDatabaseAsync();

        Guid categoryId = await Sender.CreateCategoryAsync(Faker.Music.Genre());

        for (int i = 0; i < 3; i++)
        {
            Guid eventId = await Sender.CreateEventAsync(categoryId, DateTime.UtcNow.AddDays(i + 1));
            await Sender.CreateTicketTypeAsync(eventId);
            await Sender.Send(new PublishEventCommand(eventId));
        }

        // Act
        Result<SearchEventsResponse> result = await Sender.Send(new SearchEventsQuery(categoryId, null, null, 1, 1));

        // Assert
        result.Value.Events.Should().HaveCount(1);
        result.Value.TotalCount.Should().Be(3);
        result.Value.HasMore.Should().BeTrue();
    }
}
