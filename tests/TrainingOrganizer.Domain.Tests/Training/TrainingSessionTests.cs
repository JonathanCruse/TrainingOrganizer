using FluentAssertions;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Tests.TestHelpers;
using TrainingOrganizer.Domain.Training;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.Events;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Tests.Training;

public class TrainingSessionTests
{
    // --- CreateFromTemplate ---

    [Fact]
    public void CreateFromTemplate_ValidData_CreatesSessionWithTemplateProperties()
    {
        var recurringId = RecurringTrainingId.Create();
        var timeSlot = TrainingFactory.CreateTimeSlot();
        var template = TrainingFactory.CreateTemplate();

        var session = TrainingSession.CreateFromTemplate(recurringId, timeSlot, template);

        session.Id.Should().NotBeNull();
        session.RecurringTrainingId.Should().Be(recurringId);
        session.TimeSlot.Should().Be(timeSlot);
        session.EffectiveTitle.Should().Be(template.Title);
        session.EffectiveDescription.Should().Be(template.Description);
        session.EffectiveCapacity.Should().Be(template.Capacity);
        session.EffectiveVisibility.Should().Be(template.Visibility);
        session.Status.Should().Be(SessionStatus.Scheduled);
        session.EffectiveTrainerIds.Should().BeEquivalentTo(template.TrainerIds);
        session.Participants.Should().BeEmpty();
    }

    // --- ApplyOverrides ---

    [Fact]
    public void ApplyOverrides_ScheduledSession_AppliesOverrideValues()
    {
        var session = CreateScheduledSession();
        var newTitle = new TrainingTitle("Override Title");
        var newCapacity = new Capacity(1, 5);
        var overrides = new SessionOverrides
        {
            Title = newTitle,
            Capacity = newCapacity,
            Visibility = Visibility.InviteOnly
        };

        session.ApplyOverrides(overrides);

        session.EffectiveTitle.Should().Be(newTitle);
        session.EffectiveCapacity.Should().Be(newCapacity);
        session.EffectiveVisibility.Should().Be(Visibility.InviteOnly);
        session.Overrides.Should().Be(overrides);
    }

    [Fact]
    public void ApplyOverrides_CanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        session.Cancel("Session canceled");

        var act = () => session.ApplyOverrides(new SessionOverrides { Title = new TrainingTitle("New") });

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- ResetToTemplate ---

    [Fact]
    public void ResetToTemplate_WithOverrides_ClearsOverridesAndRestoresTemplate()
    {
        var template = TrainingFactory.CreateTemplate();
        var session = CreateScheduledSession(template: template);
        session.ApplyOverrides(new SessionOverrides
        {
            Title = new TrainingTitle("Overridden Title"),
            Capacity = new Capacity(1, 100)
        });

        session.ResetToTemplate(template);

        session.EffectiveTitle.Should().Be(template.Title);
        session.EffectiveCapacity.Should().Be(template.Capacity);
        session.Overrides.Should().Be(SessionOverrides.None);
    }

    [Fact]
    public void ResetToTemplate_CanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        session.Cancel("Canceled");

        var act = () => session.ResetToTemplate(TrainingFactory.CreateTemplate());

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_ScheduledSession_TransitionsToCanceled()
    {
        var session = CreateScheduledSession();

        session.Cancel("Weather conditions");

        session.Status.Should().Be(SessionStatus.Canceled);
    }

    [Fact]
    public void Cancel_SessionWithParticipants_CancelsAllActiveParticipants()
    {
        var session = CreateScheduledSession();
        session.AddParticipant(MemberId.Create());
        session.AddParticipant(MemberId.Create());

        session.Cancel("Canceled");

        session.Participants.Should().AllSatisfy(p => p.IsActive.Should().BeFalse());
    }

    [Fact]
    public void Cancel_AlreadyCanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        session.Cancel("First cancel");

        var act = () => session.Cancel("Second cancel");

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void Cancel_ScheduledSession_RaisesTrainingSessionCanceledEvent()
    {
        var session = CreateScheduledSession();
        session.ClearDomainEvents();

        session.Cancel("Instructor sick");

        session.DomainEvents.Should().Contain(e => e is TrainingSessionCanceledEvent);
    }

    // --- Complete ---

    [Fact]
    public void Complete_ScheduledSession_TransitionsToCompleted()
    {
        var session = CreateScheduledSession();

        session.Complete();

        session.Status.Should().Be(SessionStatus.Completed);
    }

    [Fact]
    public void Complete_CanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        session.Cancel("Canceled");

        var act = () => session.Complete();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Participation ---

    [Fact]
    public void AddParticipant_CapacityAvailable_ParticipantConfirmed()
    {
        var session = CreateScheduledSession(capacity: new Capacity(1, 10));
        var memberId = MemberId.Create();

        session.AddParticipant(memberId);

        session.ConfirmedParticipantCount.Should().Be(1);
        session.Participants.Should().ContainSingle()
            .Which.IsConfirmed.Should().BeTrue();
    }

    [Fact]
    public void AddParticipant_AtCapacity_ParticipantWaitlisted()
    {
        var session = CreateScheduledSession(capacity: new Capacity(0, 1));
        session.AddParticipant(MemberId.Create());

        session.AddParticipant(MemberId.Create());

        session.WaitlistCount.Should().Be(1);
    }

    [Fact]
    public void AddParticipant_DuplicateMember_ThrowsBusinessRuleViolationException()
    {
        var session = CreateScheduledSession();
        var memberId = MemberId.Create();
        session.AddParticipant(memberId);

        var act = () => session.AddParticipant(memberId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AddParticipant_CanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        session.Cancel("Canceled");

        var act = () => session.AddParticipant(MemberId.Create());

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void RemoveParticipant_WithWaitlist_PromotesFromWaitlist()
    {
        var session = CreateScheduledSession(capacity: new Capacity(0, 1));
        var first = MemberId.Create();
        var second = MemberId.Create();
        session.AddParticipant(first);
        session.AddParticipant(second);
        session.ClearDomainEvents();

        session.RemoveParticipant(first);

        session.ConfirmedParticipantCount.Should().Be(1);
        session.WaitlistCount.Should().Be(0);
        session.DomainEvents.Should().Contain(e => e is ParticipantPromotedFromWaitlistEvent);
    }

    // --- RecordAttendance ---

    [Fact]
    public void RecordAttendance_ConfirmedParticipant_RecordsAttendance()
    {
        var session = CreateScheduledSession();
        var memberId = MemberId.Create();
        session.AddParticipant(memberId);

        session.RecordAttendance(memberId, true);

        var participant = session.Participants.First(p => p.Id == memberId);
        participant.AttendanceRecorded.Should().BeTrue();
        participant.Attended.Should().BeTrue();
    }

    [Fact]
    public void RecordAttendance_WaitlistedParticipant_ThrowsEntityNotFoundException()
    {
        var session = CreateScheduledSession(capacity: new Capacity(0, 1));
        session.AddParticipant(MemberId.Create());
        var waitlisted = MemberId.Create();
        session.AddParticipant(waitlisted);

        var act = () => session.RecordAttendance(waitlisted, true);

        act.Should().Throw<EntityNotFoundException>();
    }

    [Fact]
    public void RecordAttendance_CanceledSession_ThrowsInvalidEntityStateException()
    {
        var session = CreateScheduledSession();
        var memberId = MemberId.Create();
        session.AddParticipant(memberId);
        session.Cancel("Canceled");

        var act = () => session.RecordAttendance(memberId, true);

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Helper ---

    private static TrainingSession CreateScheduledSession(
        Capacity? capacity = null,
        TrainingTemplate? template = null)
    {
        var tmpl = template ?? TrainingFactory.CreateTemplate(capacity: capacity ?? TrainingFactory.CreateCapacity());
        return TrainingSession.CreateFromTemplate(
            RecurringTrainingId.Create(),
            TrainingFactory.CreateTimeSlot(),
            tmpl);
    }
}
