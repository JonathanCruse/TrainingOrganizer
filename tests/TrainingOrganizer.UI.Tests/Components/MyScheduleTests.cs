using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Schedule;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class MyScheduleTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();

    public MyScheduleTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new ScheduleApiClient(httpClient));
    }

    [Fact]
    public void RendersScheduleEntries_WhenLoaded()
    {
        var entries = new List<ScheduleEntryResponse>
        {
            new(Guid.NewGuid(), "Training", "Morning Yoga",
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                "Main Gym", "Studio A")
        };

        _handler.RespondWithJson("/api/v1/schedule/me", entries);

        var cut = Render<MySchedule>();

        cut.Markup.Should().Contain("Morning Yoga");
        cut.Markup.Should().Contain("Training");
        cut.Markup.Should().Contain("Main Gym");
        cut.Markup.Should().Contain("Studio A");
    }

    [Fact]
    public void ShowsDash_WhenLocationAndRoomAreNull()
    {
        var entries = new List<ScheduleEntryResponse>
        {
            new(Guid.NewGuid(), "Training", "Outdoor Run",
                DateTimeOffset.UtcNow.AddDays(1),
                DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
                null, null)
        };

        _handler.RespondWithJson("/api/v1/schedule/me", entries);

        var cut = Render<MySchedule>();

        cut.Markup.Should().Contain("Outdoor Run");
        // The component renders "—" for null location/room
        cut.Markup.Should().Contain("\u2014");
    }
}
