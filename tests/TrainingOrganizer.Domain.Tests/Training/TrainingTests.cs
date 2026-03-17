using FluentAssertions;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Tests.TestHelpers;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.Events;
using TrainingOrganizer.Domain.Training.ValueObjects;
using DomainTraining = TrainingOrganizer.Domain.Training.Training;

namespace TrainingOrganizer.Domain.Tests.Training;

public class TrainingTests
{
    // --- Create ---

    [Fact]
    public void Create_ValidData_ReturnsDraftTrainingWithCorrectProperties()
    {
        var title = new TrainingTitle("DDD Workshop");
        var description = new TrainingDescription("Learn DDD fundamentals.");
        var timeSlot = TrainingFactory.CreateTimeSlot();
        var capacity = TrainingFactory.CreateCapacity(5, 20);
        var trainerId = MemberId.Create();
        var createdBy = MemberId.Create();

        var training = DomainTraining.Create(
            title, description, timeSlot, capacity,
            Visibility.Public, [trainerId], createdBy);

        training.Id.Should().NotBeNull();
        training.Title.Should().Be(title);
        training.Description.Should().Be(description);
        training.TimeSlot.Should().Be(timeSlot);
        training.Capacity.Should().Be(capacity);
        training.Status.Should().Be(TrainingStatus.Draft);
        training.Visibility.Should().Be(Visibility.Public);
        training.TrainerIds.Should().ContainSingle().Which.Should().Be(trainerId);
        training.CreatedBy.Should().Be(createdBy);
        training.Participants.Should().BeEmpty();
    }

    [Fact]
    public void Create_ValidData_RaisesTrainingCreatedEvent()
    {
        var training = DomainTraining.Create(
            new TrainingTitle("Test"),
            new TrainingDescription("Desc"),
            TrainingFactory.CreateTimeSlot(),
            TrainingFactory.CreateCapacity(),
            Visibility.Public,
            [MemberId.Create()],
            MemberId.Create());

        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TrainingCreatedEvent>()
            .Which.TrainingId.Should().Be(training.Id);
    }

    [Fact]
    public void Create_NoTrainers_ThrowsDomainException()
    {
        var act = () => DomainTraining.Create(
            new TrainingTitle("Test"),
            new TrainingDescription("Desc"),
            TrainingFactory.CreateTimeSlot(),
            TrainingFactory.CreateCapacity(),
            Visibility.Public,
            Array.Empty<MemberId>(),
            MemberId.Create());

        act.Should().Throw<DomainException>();
    }

    // --- Publish ---

    [Fact]
    public void Publish_DraftTraining_TransitionsToPublished()
    {
        var training = TrainingFactory.CreateDraftTraining();

        training.Publish();

        training.Status.Should().Be(TrainingStatus.Published);
    }

    [Fact]
    public void Publish_DraftTraining_RaisesTrainingPublishedEventWithRoomRequirements()
    {
        var training = TrainingFactory.CreateDraftTraining();

        training.Publish();

        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<TrainingPublishedEvent>()
            .Which.RoomRequirements.Should().NotBeNull();
    }

    [Fact]
    public void Publish_PublishedTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        var act = () => training.Publish();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_PublishedTraining_TransitionsToCanceled()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        training.Cancel("Instructor unavailable");

        training.Status.Should().Be(TrainingStatus.Canceled);
    }

    [Fact]
    public void Cancel_TrainingWithParticipants_CancelsAllActiveParticipants()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);

        training.Cancel("Event canceled");

        training.Participants.Should().AllSatisfy(p => p.IsActive.Should().BeFalse());
    }

    [Fact]
    public void Cancel_PublishedTraining_RaisesTrainingCanceledEvent()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        training.Cancel("Canceled for testing");

        training.DomainEvents.Should().Contain(e => e is TrainingCanceledEvent);
    }

    [Fact]
    public void Cancel_AlreadyCanceledTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        training.Cancel("First cancel");

        var act = () => training.Cancel("Second cancel");

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Complete ---

    [Fact]
    public void Complete_PublishedTraining_TransitionsToCompleted()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        training.Complete();

        training.Status.Should().Be(TrainingStatus.Completed);
    }

    [Fact]
    public void Complete_DraftTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreateDraftTraining();

        var act = () => training.Complete();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- AddParticipant ---

    [Fact]
    public void AddParticipant_CapacityAvailable_ParticipantConfirmed()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(1, 10));
        var memberId = MemberId.Create();

        training.AddParticipant(memberId);

        training.ConfirmedParticipantCount.Should().Be(1);
        training.Participants.Should().ContainSingle()
            .Which.IsConfirmed.Should().BeTrue();
    }

    [Fact]
    public void AddParticipant_AtCapacity_ParticipantWaitlisted()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 1));
        training.AddParticipant(MemberId.Create());

        var waitlistedMember = MemberId.Create();
        training.AddParticipant(waitlistedMember);

        training.WaitlistCount.Should().Be(1);
        training.Participants.Last().IsWaitlisted.Should().BeTrue();
    }

    [Fact]
    public void AddParticipant_DuplicateMember_ThrowsBusinessRuleViolationException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);

        var act = () => training.AddParticipant(memberId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void AddParticipant_CanceledTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        training.Cancel("Event canceled");

        var act = () => training.AddParticipant(MemberId.Create());

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void AddParticipant_DraftTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreateDraftTraining();

        var act = () => training.AddParticipant(MemberId.Create());

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- RemoveParticipant ---

    [Fact]
    public void RemoveParticipant_ConfirmedParticipant_CancelsParticipant()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);

        training.RemoveParticipant(memberId);

        training.ConfirmedParticipantCount.Should().Be(0);
    }

    [Fact]
    public void RemoveParticipant_WithWaitlistedMembers_PromotesFromWaitlist()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 1));
        var firstMember = MemberId.Create();
        var waitlistedMember = MemberId.Create();
        training.AddParticipant(firstMember);
        training.AddParticipant(waitlistedMember);
        training.ClearDomainEvents();

        training.RemoveParticipant(firstMember);

        training.ConfirmedParticipantCount.Should().Be(1);
        training.WaitlistCount.Should().Be(0);
        training.DomainEvents.Should().Contain(e => e is ParticipantPromotedFromWaitlistEvent);
    }

    // --- RecordAttendance ---

    [Fact]
    public void RecordAttendance_ConfirmedParticipant_RecordsAttendance()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);

        training.RecordAttendance(memberId, true);

        var participant = training.Participants.First(p => p.Id == memberId);
        participant.AttendanceRecorded.Should().BeTrue();
        participant.Attended.Should().BeTrue();
    }

    [Fact]
    public void RecordAttendance_NonConfirmedParticipant_ThrowsException()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 1));
        var confirmedMember = MemberId.Create();
        var waitlistedMember = MemberId.Create();
        training.AddParticipant(confirmedMember);
        training.AddParticipant(waitlistedMember);

        var act = () => training.RecordAttendance(waitlistedMember, true);

        act.Should().Throw<EntityNotFoundException>();
    }

    [Fact]
    public void RecordAttendance_CanceledTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);
        training.Cancel("Canceled");

        var act = () => training.RecordAttendance(memberId, true);

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- RequestGuestParticipation ---

    [Fact]
    public void RequestGuestParticipation_PublishedTraining_CreatesPendingApprovalParticipant()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();

        training.RequestGuestParticipation(guestId);

        training.Participants.Should().ContainSingle()
            .Which.IsPendingApproval.Should().BeTrue();
        training.PendingApprovalCount.Should().Be(1);
        training.ConfirmedParticipantCount.Should().Be(0);
    }

    [Fact]
    public void RequestGuestParticipation_PublishedTraining_RaisesGuestParticipationRequestedEvent()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();

        training.RequestGuestParticipation(guestId);

        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GuestParticipationRequestedEvent>()
            .Which.MemberId.Should().Be(guestId);
    }

    [Fact]
    public void RequestGuestParticipation_DraftTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreateDraftTraining();

        var act = () => training.RequestGuestParticipation(MemberId.Create());

        act.Should().Throw<InvalidEntityStateException>();
    }

    [Fact]
    public void RequestGuestParticipation_DuplicateGuest_ThrowsBusinessRuleViolationException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);

        var act = () => training.RequestGuestParticipation(guestId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RequestGuestParticipation_AlreadyConfirmedMember_ThrowsBusinessRuleViolationException()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var memberId = MemberId.Create();
        training.AddParticipant(memberId);

        var act = () => training.RequestGuestParticipation(memberId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    [Fact]
    public void RequestGuestParticipation_DoesNotCountTowardCapacity()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 1));
        training.RequestGuestParticipation(MemberId.Create());

        // Adding a regular participant should still work — guest doesn't consume capacity
        training.AddParticipant(MemberId.Create());

        training.ConfirmedParticipantCount.Should().Be(1);
        training.PendingApprovalCount.Should().Be(1);
    }

    // --- AcceptParticipant ---

    [Fact]
    public void AcceptParticipant_WithCapacity_ConfirmsParticipant()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 10));
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);
        training.ClearDomainEvents();

        training.AcceptParticipant(guestId);

        var participant = training.Participants.First(p => p.Id == guestId);
        participant.IsConfirmed.Should().BeTrue();
        training.ConfirmedParticipantCount.Should().Be(1);
        training.PendingApprovalCount.Should().Be(0);
    }

    [Fact]
    public void AcceptParticipant_AtCapacity_WaitlistsParticipant()
    {
        var training = TrainingFactory.CreatePublishedTraining(capacity: new Capacity(0, 1));
        training.AddParticipant(MemberId.Create()); // fill capacity
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);
        training.ClearDomainEvents();

        training.AcceptParticipant(guestId);

        var participant = training.Participants.First(p => p.Id == guestId);
        participant.IsWaitlisted.Should().BeTrue();
        training.WaitlistCount.Should().Be(1);
        training.PendingApprovalCount.Should().Be(0);
    }

    [Fact]
    public void AcceptParticipant_RaisesGuestParticipantAcceptedEvent()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);
        training.ClearDomainEvents();

        training.AcceptParticipant(guestId);

        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GuestParticipantAcceptedEvent>();
    }

    [Fact]
    public void AcceptParticipant_NonPendingMember_ThrowsEntityNotFoundException()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        var act = () => training.AcceptParticipant(MemberId.Create());

        act.Should().Throw<EntityNotFoundException>();
    }

    // --- RejectParticipant ---

    [Fact]
    public void RejectParticipant_PendingGuest_CancelsParticipant()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);
        training.ClearDomainEvents();

        training.RejectParticipant(guestId);

        var participant = training.Participants.First(p => p.Id == guestId);
        participant.Status.Should().Be(ParticipationStatus.Canceled);
        training.PendingApprovalCount.Should().Be(0);
    }

    [Fact]
    public void RejectParticipant_RaisesGuestParticipantRejectedEvent()
    {
        var training = TrainingFactory.CreatePublishedTraining();
        var guestId = MemberId.Create();
        training.RequestGuestParticipation(guestId);
        training.ClearDomainEvents();

        training.RejectParticipant(guestId);

        training.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<GuestParticipantRejectedEvent>()
            .Which.MemberId.Should().Be(guestId);
    }

    [Fact]
    public void RejectParticipant_NonPendingMember_ThrowsEntityNotFoundException()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        var act = () => training.RejectParticipant(MemberId.Create());

        act.Should().Throw<EntityNotFoundException>();
    }

    // --- AssignTrainer ---

    [Fact]
    public void AssignTrainer_NewTrainer_AddsTrainer()
    {
        var training = TrainingFactory.CreateDraftTraining();
        var newTrainerId = MemberId.Create();

        training.AssignTrainer(newTrainerId);

        training.TrainerIds.Should().Contain(newTrainerId);
    }

    [Fact]
    public void AssignTrainer_DuplicateTrainer_ThrowsBusinessRuleViolationException()
    {
        var trainerId = MemberId.Create();
        var training = TrainingFactory.CreateDraftTraining(trainerIds: [trainerId]);

        var act = () => training.AssignTrainer(trainerId);

        act.Should().Throw<BusinessRuleViolationException>();
    }

    // --- RemoveTrainer ---

    [Fact]
    public void RemoveTrainer_WithMultipleTrainers_RemovesTrainer()
    {
        var trainer1 = MemberId.Create();
        var trainer2 = MemberId.Create();
        var training = TrainingFactory.CreateDraftTraining(trainerIds: [trainer1, trainer2]);

        training.RemoveTrainer(trainer1);

        training.TrainerIds.Should().NotContain(trainer1);
        training.TrainerIds.Should().Contain(trainer2);
    }

    [Fact]
    public void RemoveTrainer_LastTrainer_ThrowsDomainException()
    {
        var trainerId = MemberId.Create();
        var training = TrainingFactory.CreateDraftTraining(trainerIds: [trainerId]);

        var act = () => training.RemoveTrainer(trainerId);

        act.Should().Throw<DomainException>();
    }

    [Fact]
    public void RemoveTrainer_NonExistentTrainer_ThrowsEntityNotFoundException()
    {
        var training = TrainingFactory.CreateDraftTraining();

        var act = () => training.RemoveTrainer(MemberId.Create());

        act.Should().Throw<EntityNotFoundException>();
    }

    // --- Update ---

    [Fact]
    public void Update_DraftTraining_ModifiesProperties()
    {
        var training = TrainingFactory.CreateDraftTraining();
        var newTitle = new TrainingTitle("Updated Workshop");
        var newDescription = new TrainingDescription("Updated description.");
        var newTimeSlot = TrainingFactory.CreateTimeSlot(DateTimeOffset.UtcNow.AddDays(14));
        var newCapacity = new Capacity(3, 30);

        training.Update(newTitle, newDescription, newTimeSlot, newCapacity, Visibility.MembersOnly);

        training.Title.Should().Be(newTitle);
        training.Description.Should().Be(newDescription);
        training.TimeSlot.Should().Be(newTimeSlot);
        training.Capacity.Should().Be(newCapacity);
        training.Visibility.Should().Be(Visibility.MembersOnly);
    }

    [Fact]
    public void Update_PublishedTraining_ThrowsInvalidEntityStateException()
    {
        var training = TrainingFactory.CreatePublishedTraining();

        var act = () => training.Update(
            new TrainingTitle("New Title"),
            new TrainingDescription("New Desc"),
            TrainingFactory.CreateTimeSlot(),
            TrainingFactory.CreateCapacity(),
            Visibility.Public);

        act.Should().Throw<InvalidEntityStateException>();
    }
}
