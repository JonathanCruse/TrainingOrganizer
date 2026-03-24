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

public sealed class SessionDetailTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly BunitAuthorizationContext _authContext;
    private readonly Guid _sessionId = Guid.NewGuid();
    private readonly Guid _memberId = Guid.NewGuid();

    public SessionDetailTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new SessionApiClient(httpClient));
        Services.AddSingleton(new MemberApiClient(httpClient));
        _authContext = this.AddAuthorization();
        _authContext.SetAuthorized("user@test.com");
        _authContext.SetClaims(new System.Security.Claims.Claim("member_id", _memberId.ToString()));
        Render<MudPopoverProvider>();
    }

    private void SetRole(string role)
    {
        _authContext.SetRoles(role);
        _authContext.SetClaims(new System.Security.Claims.Claim("member_id", _memberId.ToString()));
    }

    [Fact]
    public void RendersSessionTitle_WhenLoaded()
    {
        var session = CreateSession(title: "Wednesday Boxing");
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Wednesday Boxing");
    }

    [Fact]
    public void ShowsDescription_WhenPresent()
    {
        var session = CreateSession(description: "Beginner-friendly boxing session");
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Beginner-friendly boxing session");
    }

    [Fact]
    public void ShowsParticipantCounts()
    {
        var session = CreateSession(confirmedCount: 12, maxCapacity: 15, waitlistCount: 3);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("12 / 15 confirmed");
        cut.Markup.Should().Contain("3 on waitlist");
    }

    [Fact]
    public void ShowsJoinButton_WhenScheduledAndNotParticipant()
    {
        var session = CreateSession(status: SessionStatus.Scheduled);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Join Session");
    }

    [Fact]
    public void ShowsLeaveButton_WhenConfirmed()
    {
        var participant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [participant]);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("You are confirmed");
        cut.Markup.Should().Contain("Leave");
    }

    [Fact]
    public void ShowsWaitlistPosition_WhenWaitlisted()
    {
        var participant = new ParticipantResponse(
            _memberId, ParticipationStatus.Waitlisted, DateTimeOffset.UtcNow, 2, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [participant]);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Waitlist position 2");
        cut.Markup.Should().Contain("Leave Waitlist");
    }

    [Fact]
    public void ShowsAwaitingApproval_WhenPending()
    {
        var participant = new ParticipantResponse(
            _memberId, ParticipationStatus.PendingApproval, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [participant]);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Awaiting approval");
    }

    [Fact]
    public void HidesJoinButton_WhenSessionCanceled()
    {
        var session = CreateSession(status: SessionStatus.Canceled);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().NotContain("Join Session");
    }

    [Fact]
    public void ShowsCompleteAndCancelButtons_ForTrainer_WhenScheduled()
    {
        SetRole("Trainer");
        SetupMemberList();

        var session = CreateSession(status: SessionStatus.Scheduled);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Complete");
        cut.Markup.Should().Contain("Cancel");
    }

    [Fact]
    public void HidesCompleteAndCancelButtons_WhenSessionCompleted()
    {
        SetRole("Trainer");
        SetupMemberList();

        var session = CreateSession(status: SessionStatus.Completed);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().NotContain(">Complete<");
        cut.Markup.Should().NotContain(">Cancel<");
    }

    [Fact]
    public void ShowsPendingParticipants_ForTrainer()
    {
        SetRole("Trainer");

        var pendingMemberId = Guid.NewGuid();
        var pending = new ParticipantResponse(
            pendingMemberId, ParticipationStatus.PendingApproval, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [pending]);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);
        SetupMemberList(new MemberResponse(
            pendingMemberId, "Jane", "Doe", "jane@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Pending Approval");
        cut.Markup.Should().Contain("Jane Doe");
    }

    [Fact]
    public void ShowsParticipantNames_ForTrainer()
    {
        SetRole("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "John", "Smith", "john@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Attendance (1)");
        cut.Markup.Should().Contain("John Smith");
    }

    [Fact]
    public void ShowsOverriddenChip_WhenSessionHasOverrides()
    {
        var session = CreateSession(hasOverrides: true);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Overridden");
    }

    [Fact]
    public void ShowsStatusChips()
    {
        var session = CreateSession(status: SessionStatus.Scheduled, visibility: Visibility.MembersOnly);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Scheduled");
        cut.Markup.Should().Contain("MembersOnly");
    }

    [Fact]
    public void ShowsAttendanceTable_ForTrainer_WhenScheduled()
    {
        SetRole("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "Alice", "Trainer", "alice@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Attendance (1)");
        cut.Markup.Should().Contain("Save Attendance");
        cut.Markup.Should().Contain("Alice Trainer");
    }

    [Fact]
    public void ShowsAttendanceTable_ForTrainer_WhenCompleted()
    {
        SetRole("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, true);
        var session = CreateSession(
            status: SessionStatus.Completed,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "Bob", "Member", "bob@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("Attendance (1)");
        cut.Markup.Should().Contain("Save Attendance");
    }

    [Fact]
    public void HidesAttendanceTable_WhenSessionCanceledStatus()
    {
        SetRole("Trainer");

        var session = CreateSession(status: SessionStatus.Canceled);
        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);
        SetupMemberList();

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().NotContain("Save Attendance");
    }

    [Fact]
    public void HidesAttendanceTable_ForRegularMember()
    {
        SetRole("Member");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().NotContain("Save Attendance");
    }

    [Fact]
    public void ShowsOwnAttendanceStatus_WhenRecorded()
    {
        SetRole("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, true);
        var session = CreateSession(
            status: SessionStatus.Completed,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("You attended this session.");
    }

    [Fact]
    public void ShowsAbsentStatus_WhenRecordedAsAbsent()
    {
        SetRole("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, false);
        var session = CreateSession(
            status: SessionStatus.Completed,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().Contain("You did not attend this session.");
    }

    [Fact]
    public void HidesAttendanceStatus_WhenNotRecorded()
    {
        SetRole("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var session = CreateSession(
            status: SessionStatus.Scheduled,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/sessions/{_sessionId}", session);

        var cut = Render<SessionDetail>(p => p.Add(x => x.Id, _sessionId));

        cut.Markup.Should().NotContain("You attended this session.");
        cut.Markup.Should().NotContain("You did not attend this session.");
    }

    private void SetupMemberList(params MemberResponse[] members)
    {
        var memberList = new PagedResponse<MemberResponse>(
            Items: members,
            Page: 1, PageSize: 200, TotalCount: members.Length, TotalPages: 1,
            HasNextPage: false, HasPreviousPage: false);
        _handler.RespondWithJson("/api/v1/members", memberList);
    }

    private TrainingSessionResponse CreateSession(
        string title = "Test Session",
        string description = "Description",
        SessionStatus status = SessionStatus.Scheduled,
        Visibility visibility = Visibility.Public,
        int confirmedCount = 0,
        int maxCapacity = 20,
        int waitlistCount = 0,
        bool hasOverrides = false,
        IReadOnlyList<ParticipantResponse>? participants = null)
    {
        return new TrainingSessionResponse(
            Id: _sessionId,
            RecurringTrainingId: Guid.NewGuid(),
            Title: title,
            Description: description,
            Start: DateTimeOffset.UtcNow.AddDays(1),
            End: DateTimeOffset.UtcNow.AddDays(1).AddHours(1),
            MinCapacity: 5,
            MaxCapacity: maxCapacity,
            Visibility: visibility,
            Status: status,
            TrainerIds: [Guid.NewGuid()],
            Participants: participants ?? [],
            RoomRequirements: [],
            HasOverrides: hasOverrides,
            ConfirmedParticipantCount: confirmedCount,
            WaitlistCount: waitlistCount,
            CreatedAt: DateTimeOffset.UtcNow);
    }
}
