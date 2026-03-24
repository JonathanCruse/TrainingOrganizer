using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Sessions;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class SessionListTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly BunitAuthorizationContext _authContext;
    private readonly Guid _memberId = Guid.NewGuid();

    public SessionListTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new SessionApiClient(httpClient));
        _authContext = this.AddAuthorization();
        _authContext.SetAuthorized("user@test.com");
        _authContext.SetClaims(new System.Security.Claims.Claim("member_id", _memberId.ToString()));
        Render<MudPopoverProvider>();
    }

    [Fact]
    public void RendersSessionCards_WhenLoaded()
    {
        var sessions = CreatePagedResponse(CreateSession(
            title: "Monday Yoga",
            confirmedCount: 8,
            maxCapacity: 20));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Monday Yoga");
        cut.Markup.Should().Contain("8 / 20");
    }

    [Fact]
    public void ShowsWaitlistCount_WhenPositive()
    {
        var sessions = CreatePagedResponse(CreateSession(
            title: "Full Session",
            confirmedCount: 20,
            maxCapacity: 20,
            waitlistCount: 5));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Waitlist: 5");
    }

    [Fact]
    public void HidesWaitlistCount_WhenZero()
    {
        var sessions = CreatePagedResponse(CreateSession(
            title: "Open Session",
            confirmedCount: 5,
            maxCapacity: 20,
            waitlistCount: 0));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().NotContain("Waitlist");
    }

    [Fact]
    public void ShowsNoSessionsMessage_WhenEmpty()
    {
        _handler.RespondWithJson("/api/v1/sessions", CreatePagedResponse());

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("No sessions found.");
    }

    [Fact]
    public void ShowsJoinButton_ForScheduledSessionWhenNotParticipant()
    {
        var sessions = CreatePagedResponse(CreateSession(
            title: "Open Session",
            status: SessionStatus.Scheduled));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Join");
    }

    [Fact]
    public void ShowsConfirmedChip_WhenMemberIsConfirmed()
    {
        var participant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var sessions = CreatePagedResponse(CreateSession(
            title: "My Session",
            status: SessionStatus.Scheduled,
            participants: [participant]));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Confirmed");
        cut.Markup.Should().NotContain(">Join<");
    }

    [Fact]
    public void ShowsStatusChipForCanceledSession()
    {
        var sessions = CreatePagedResponse(CreateSession(
            title: "Canceled Session",
            status: SessionStatus.Canceled));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Canceled");
    }

    [Fact]
    public void ShowsBackButton_WhenFilteredByRecurringTraining()
    {
        var recurringId = Guid.NewGuid();
        _handler.RespondWithJson("/api/v1/sessions", CreatePagedResponse());

        var cut = Render<SessionList>(p => p.Add(x => x.RecurringTrainingId, recurringId));

        cut.Markup.Should().Contain("Back");
    }

    [Fact]
    public void DoesNotShowBackButton_WhenNoRecurringTrainingFilter()
    {
        _handler.RespondWithJson("/api/v1/sessions", CreatePagedResponse());

        var cut = Render<SessionList>();

        cut.Markup.Should().NotContain("arrow_back");
    }

    [Fact]
    public void ShowsMultipleSessions()
    {
        var sessions = CreatePagedResponse(
            CreateSession(title: "Session A"),
            CreateSession(title: "Session B"));

        _handler.RespondWithJson("/api/v1/sessions", sessions);

        var cut = Render<SessionList>();

        cut.Markup.Should().Contain("Session A");
        cut.Markup.Should().Contain("Session B");
    }

    private static TrainingSessionResponse CreateSession(
        Guid? id = null,
        string title = "Session",
        SessionStatus status = SessionStatus.Scheduled,
        int confirmedCount = 0,
        int maxCapacity = 20,
        int waitlistCount = 0,
        IReadOnlyList<ParticipantResponse>? participants = null)
    {
        return new TrainingSessionResponse(
            Id: id ?? Guid.NewGuid(),
            RecurringTrainingId: Guid.NewGuid(),
            Title: title,
            Description: "Description",
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: maxCapacity,
            Visibility: Visibility.Public,
            Status: status,
            TrainerIds: [Guid.NewGuid()],
            Participants: participants ?? [],
            RoomRequirements: [],
            HasOverrides: false,
            ConfirmedParticipantCount: confirmedCount,
            WaitlistCount: waitlistCount,
            CreatedAt: DateTimeOffset.UtcNow);
    }

    private static PagedResponse<TrainingSessionResponse> CreatePagedResponse(
        params TrainingSessionResponse[] items)
    {
        return new PagedResponse<TrainingSessionResponse>(
            Items: items,
            Page: 1,
            PageSize: 20,
            TotalCount: items.Length,
            TotalPages: 1,
            HasNextPage: false,
            HasPreviousPage: false);
    }
}
