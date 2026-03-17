using Bunit;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Trainings;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class TrainingListTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();

    public TrainingListTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new TrainingApiClient(httpClient));
    }

    [Fact]
    public void RendersTrainingCards_WhenLoaded()
    {
        var trainings = CreatePagedResponse(CreateTraining(
            title: "Yoga Class",
            description: "Beginner yoga session",
            confirmedCount: 8,
            maxCapacity: 20));

        _handler.RespondWithJson("/api/v1/trainings?page=1&pageSize=20", trainings);

        var cut = Render<TrainingList>();

        cut.Markup.Should().Contain("Yoga Class");
        cut.Markup.Should().Contain("Beginner yoga session");
        cut.Markup.Should().Contain("8 / 20");
    }

    [Fact]
    public void ShowsWaitlistCount_WhenPositive()
    {
        var trainings = CreatePagedResponse(CreateTraining(
            title: "Full Class",
            confirmedCount: 20,
            maxCapacity: 20,
            waitlistCount: 3));

        _handler.RespondWithJson("/api/v1/trainings?page=1&pageSize=20", trainings);

        var cut = Render<TrainingList>();

        cut.Markup.Should().Contain("Waitlist: 3");
    }

    [Fact]
    public void HidesWaitlistCount_WhenZero()
    {
        var trainings = CreatePagedResponse(CreateTraining(
            title: "Open Class",
            confirmedCount: 5,
            maxCapacity: 20,
            waitlistCount: 0));

        _handler.RespondWithJson("/api/v1/trainings?page=1&pageSize=20", trainings);

        var cut = Render<TrainingList>();

        cut.Markup.Should().NotContain("Waitlist");
    }

    private static TrainingResponse CreateTraining(
        Guid? id = null,
        string title = "Training",
        string description = "Description",
        TrainingStatus status = TrainingStatus.Published,
        int confirmedCount = 0,
        int maxCapacity = 20,
        int waitlistCount = 0)
    {
        return new TrainingResponse(
            Id: id ?? Guid.NewGuid(),
            Title: title,
            Description: description,
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: maxCapacity,
            Visibility: Visibility.Public,
            Status: status,
            TrainerIds: [Guid.NewGuid()],
            Participants: [],
            RoomRequirements: [],
            ConfirmedParticipantCount: confirmedCount,
            WaitlistCount: waitlistCount,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: Guid.NewGuid());
    }

    private static PagedResponse<TrainingResponse> CreatePagedResponse(params TrainingResponse[] items)
    {
        return new PagedResponse<TrainingResponse>(
            Items: items,
            Page: 1,
            PageSize: 20,
            TotalCount: items.Length,
            TotalPages: 1,
            HasNextPage: false,
            HasPreviousPage: false);
    }
}
