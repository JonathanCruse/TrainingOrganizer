using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Entities;
using TrainingOrganizer.Domain.Training.Enums;
using TrainingOrganizer.Domain.Training.Events;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training;

public sealed class Training : AggregateRoot<TrainingId>
{
    private readonly List<Participant> _participants = [];
    private readonly List<MemberId> _trainerIds = [];
    private readonly List<RoomRequirement> _roomRequirements = [];
    private readonly ParticipantManager _participantManager;

    private TrainingTitle _title;
    private TrainingDescription _description;
    private TimeSlot _timeSlot;
    private Capacity _capacity;

    public required TrainingTitle Title { get => _title; init => _title = value; }
    public required TrainingDescription Description { get => _description; init => _description = value; }
    public required TimeSlot TimeSlot { get => _timeSlot; init => _timeSlot = value; }
    public required Capacity Capacity { get => _capacity; init => _capacity = value; }
    public Visibility Visibility { get; private set; }
    public TrainingStatus Status { get; private set; }
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();
    public IReadOnlyList<MemberId> TrainerIds => _trainerIds.AsReadOnly();
    public IReadOnlyList<RoomRequirement> RoomRequirements => _roomRequirements.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }
    public required MemberId CreatedBy { get; init; }

    [SetsRequiredMembers]
    private Training()
    {
        _title = default!;
        _description = default!;
        _timeSlot = default!;
        _capacity = default!;
        _participantManager = new ParticipantManager(_participants);
    }

    public static Training Create(
        TrainingTitle title,
        TrainingDescription description,
        TimeSlot timeSlot,
        Capacity capacity,
        Visibility visibility,
        IReadOnlyList<MemberId> trainerIds,
        MemberId createdBy)
    {
        Guard.AgainstNull(title, nameof(title));
        Guard.AgainstNull(description, nameof(description));
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        Guard.AgainstNull(capacity, nameof(capacity));
        Guard.AgainstNull(trainerIds, nameof(trainerIds));
        Guard.AgainstCondition(trainerIds.Count == 0, "A training must have at least one trainer.");
        Guard.AgainstNull(createdBy, nameof(createdBy));

        var training = new Training
        {
            Id = TrainingId.Create(),
            Title = title,
            Description = description,
            TimeSlot = timeSlot,
            Capacity = capacity,
            Visibility = visibility,
            Status = TrainingStatus.Draft,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        training._trainerIds.AddRange(trainerIds);
        training.AddDomainEvent(new TrainingCreatedEvent(training.Id, DateTimeOffset.UtcNow));

        return training;
    }

    public void Publish()
    {
        if (Status != TrainingStatus.Draft)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "publish");

        Guard.AgainstCondition(_trainerIds.Count == 0, "Cannot publish a training without at least one trainer.");

        Status = TrainingStatus.Published;

        AddDomainEvent(new TrainingPublishedEvent(Id, _roomRequirements.AsReadOnly(), DateTimeOffset.UtcNow));
    }

    public void Cancel(string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        if (Status is TrainingStatus.Canceled or TrainingStatus.Completed)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "cancel");

        Status = TrainingStatus.Canceled;

        foreach (var p in _participants.Where(p => p.IsActive))
            p.Cancel();

        AddDomainEvent(new TrainingCanceledEvent(Id, reason, DateTimeOffset.UtcNow));
    }

    public void Complete()
    {
        if (Status != TrainingStatus.Published)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "complete");

        Status = TrainingStatus.Completed;

        AddDomainEvent(new TrainingCompletedEvent(Id, DateTimeOffset.UtcNow));
    }

    public void AddParticipant(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status != TrainingStatus.Published)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "add participant");

        var (participant, wasWaitlisted) = _participantManager.AddParticipant(memberId, Capacity);

        AddDomainEvent(new ParticipantJoinedEvent(
            Id.Value,
            memberId,
            wasWaitlisted ? ParticipationStatus.Waitlisted : ParticipationStatus.Confirmed,
            DateTimeOffset.UtcNow));
    }

    public void RequestGuestParticipation(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status != TrainingStatus.Published)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "request guest participation");

        _participantManager.AddGuestParticipant(memberId);

        AddDomainEvent(new GuestParticipationRequestedEvent(Id.Value, memberId, DateTimeOffset.UtcNow));
    }

    public void AcceptParticipant(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status != TrainingStatus.Published)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "accept participant");

        var (participant, wasWaitlisted) = _participantManager.AcceptParticipant(memberId, Capacity);

        AddDomainEvent(new GuestParticipantAcceptedEvent(
            Id.Value,
            memberId,
            wasWaitlisted ? ParticipationStatus.Waitlisted : ParticipationStatus.Confirmed,
            DateTimeOffset.UtcNow));
    }

    public void RejectParticipant(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status != TrainingStatus.Published)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "reject participant");

        _participantManager.RejectParticipant(memberId);

        AddDomainEvent(new GuestParticipantRejectedEvent(Id.Value, memberId, DateTimeOffset.UtcNow));
    }

    public void RemoveParticipant(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        var (canceled, promoted) = _participantManager.RemoveParticipant(memberId);

        AddDomainEvent(new ParticipantCanceledEvent(Id.Value, memberId, DateTimeOffset.UtcNow));

        if (promoted is not null)
        {
            AddDomainEvent(new ParticipantPromotedFromWaitlistEvent(
                Id.Value, promoted.Id, DateTimeOffset.UtcNow));
        }
    }

    public void RecordAttendance(MemberId memberId, bool attended)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status is not (TrainingStatus.Published or TrainingStatus.Completed))
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "record attendance");

        _participantManager.RecordAttendance(memberId, attended);

        AddDomainEvent(new AttendanceRecordedEvent(Id.Value, memberId, attended, DateTimeOffset.UtcNow));
    }

    public void AssignTrainer(MemberId trainerId)
    {
        Guard.AgainstNull(trainerId, nameof(trainerId));

        if (_trainerIds.Contains(trainerId))
            throw new BusinessRuleViolationException("DuplicateTrainer", "Trainer is already assigned.");

        _trainerIds.Add(trainerId);
    }

    public void RemoveTrainer(MemberId trainerId)
    {
        Guard.AgainstNull(trainerId, nameof(trainerId));

        if (!_trainerIds.Remove(trainerId))
            throw new EntityNotFoundException("Trainer", trainerId);

        Guard.AgainstCondition(_trainerIds.Count == 0, "A training must have at least one trainer.");
    }

    public void AddRoomRequirement(RoomRequirement requirement)
    {
        Guard.AgainstNull(requirement, nameof(requirement));

        if (_roomRequirements.Any(r => r.RoomId == requirement.RoomId))
            throw new BusinessRuleViolationException("DuplicateRoom", "Room is already required for this training.");

        _roomRequirements.Add(requirement);
    }

    public void RemoveRoomRequirement(RoomRequirement requirement)
    {
        Guard.AgainstNull(requirement, nameof(requirement));

        if (!_roomRequirements.Remove(requirement))
            throw new EntityNotFoundException("RoomRequirement", requirement.RoomId);
    }

    public void Update(TrainingTitle title, TrainingDescription description, TimeSlot timeSlot, Capacity capacity, Visibility visibility)
    {
        if (Status != TrainingStatus.Draft)
            throw new InvalidEntityStateException(nameof(Training), Status.ToString(), "update");

        Guard.AgainstNull(title, nameof(title));
        Guard.AgainstNull(description, nameof(description));
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        Guard.AgainstNull(capacity, nameof(capacity));

        _title = title;
        _description = description;
        _timeSlot = timeSlot;
        _capacity = capacity;
        Visibility = visibility;
    }

    public int ConfirmedParticipantCount => _participantManager.ConfirmedCount;
    public int WaitlistCount => _participantManager.WaitlistCount;
    public int PendingApprovalCount => _participantManager.PendingApprovalCount;
}
