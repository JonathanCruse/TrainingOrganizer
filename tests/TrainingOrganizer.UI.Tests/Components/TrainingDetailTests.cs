using Bunit;
using Bunit.TestDoubles;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MudBlazor;
using TrainingOrganizer.Shared.Enums;
using TrainingOrganizer.Shared.Models;
using TrainingOrganizer.UI.Pages.Trainings;
using TrainingOrganizer.UI.Services;
using TrainingOrganizer.UI.Tests.Helpers;

namespace TrainingOrganizer.UI.Tests.Components;

public sealed class TrainingDetailTests : BunitTestBase
{
    private readonly MockHttpMessageHandler _handler = new();
    private readonly BunitAuthorizationContext _authContext;
    private readonly Guid _trainingId = Guid.NewGuid();
    private readonly Guid _memberId = Guid.NewGuid();

    public TrainingDetailTests()
    {
        var httpClient = CreateMockHttpClient(_handler);
        Services.AddSingleton(new TrainingApiClient(httpClient));
        Services.AddSingleton(new MemberApiClient(httpClient));
        _authContext = this.AddAuthorization();
        _authContext.SetAuthorized("user@test.com");
        _authContext.SetClaims(new System.Security.Claims.Claim("member_id", _memberId.ToString()));
        Render<MudPopoverProvider>();
    }

    [Fact]
    public void ShowsAttendanceTable_ForTrainer_WhenPublished()
    {
        SetupAuth("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var training = CreateTraining(
            status: TrainingStatus.Published,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "Alice", "Trainer", "alice@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("Attendance (1)");
        cut.Markup.Should().Contain("Save Attendance");
        cut.Markup.Should().Contain("Alice Trainer");
    }

    [Fact]
    public void ShowsAttendanceTable_ForTrainer_WhenCompleted()
    {
        SetupAuth("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, true);
        var training = CreateTraining(
            status: TrainingStatus.Completed,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "Bob", "Member", "bob@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("Attendance (1)");
        cut.Markup.Should().Contain("Save Attendance");
    }

    [Fact]
    public void HidesAttendanceTable_WhenDraft()
    {
        SetupAuth("Trainer");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var training = CreateTraining(
            status: TrainingStatus.Draft,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);
        SetupMemberList(new MemberResponse(
            confirmedMemberId, "Charlie", "Test", "charlie@test.com", null,
            [MemberRole.Member], RegistrationStatus.Approved, DateTimeOffset.UtcNow));

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().NotContain("Save Attendance");
    }

    [Fact]
    public void HidesAttendanceTable_WhenCanceled()
    {
        SetupAuth("Trainer");

        var training = CreateTraining(status: TrainingStatus.Canceled);
        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);
        SetupMemberList();

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().NotContain("Save Attendance");
    }

    [Fact]
    public void HidesAttendanceTable_ForRegularMember()
    {
        SetupAuth("Member");

        var confirmedMemberId = Guid.NewGuid();
        var confirmed = new ParticipantResponse(
            confirmedMemberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var training = CreateTraining(
            status: TrainingStatus.Published,
            participants: [confirmed],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().NotContain("Save Attendance");
    }

    [Fact]
    public void ShowsOwnAttendanceStatus_WhenRecorded()
    {
        SetupAuth("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, true);
        var training = CreateTraining(
            status: TrainingStatus.Completed,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("You attended this training.");
    }

    [Fact]
    public void ShowsAbsentStatus_WhenRecordedAsAbsent()
    {
        SetupAuth("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, true, false);
        var training = CreateTraining(
            status: TrainingStatus.Completed,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("You did not attend this training.");
    }

    [Fact]
    public void HidesAttendanceStatus_WhenNotRecorded()
    {
        SetupAuth("Member");

        var myParticipant = new ParticipantResponse(
            _memberId, ParticipationStatus.Confirmed, DateTimeOffset.UtcNow, null, false, false);
        var training = CreateTraining(
            status: TrainingStatus.Published,
            participants: [myParticipant],
            confirmedCount: 1);

        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().NotContain("You attended this training.");
        cut.Markup.Should().NotContain("You did not attend this training.");
    }

    [Fact]
    public void ShowsTrainingTitle()
    {
        SetupAuth("Member");

        var training = CreateTraining(title: "Kickboxing Session");
        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("Kickboxing Session");
    }

    [Fact]
    public void ShowsParticipantCounts()
    {
        SetupAuth("Member");

        var training = CreateTraining(confirmedCount: 10, maxCapacity: 15, waitlistCount: 2);
        _handler.RespondWithJson($"/api/v1/trainings/{_trainingId}", training);

        var cut = Render<TrainingDetail>(p => p.Add(x => x.Id, _trainingId));

        cut.Markup.Should().Contain("10 / 15 confirmed");
        cut.Markup.Should().Contain("2 on waitlist");
    }

    private void SetupAuth(string role)
    {
        _authContext.SetRoles(role);
        _authContext.SetClaims(new System.Security.Claims.Claim("member_id", _memberId.ToString()));
    }

    private void SetupMemberList(params MemberResponse[] members)
    {
        var memberList = new PagedResponse<MemberResponse>(
            Items: members,
            Page: 1, PageSize: 200, TotalCount: members.Length, TotalPages: 1,
            HasNextPage: false, HasPreviousPage: false);
        _handler.RespondWithJson("/api/v1/members", memberList);
    }

    private TrainingResponse CreateTraining(
        string title = "Test Training",
        string description = "Description",
        TrainingStatus status = TrainingStatus.Published,
        Visibility visibility = Visibility.Public,
        int confirmedCount = 0,
        int maxCapacity = 20,
        int waitlistCount = 0,
        IReadOnlyList<ParticipantResponse>? participants = null)
    {
        return new TrainingResponse(
            Id: _trainingId,
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
            ConfirmedParticipantCount: confirmedCount,
            WaitlistCount: waitlistCount,
            CreatedAt: DateTimeOffset.UtcNow,
            CreatedBy: Guid.NewGuid());
    }
}
