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

public sealed class TrainingSession : AggregateRoot<TrainingSessionId>
{
    private readonly List<Participant> _participants = [];
    private readonly List<MemberId> _effectiveTrainerIds = [];
    private readonly List<RoomRequirement> _effectiveRoomRequirements = [];
    private readonly ParticipantManager _participantManager;

    private TrainingTitle _effectiveTitle;
    private TrainingDescription _effectiveDescription;
    private TimeSlot _timeSlot;
    private Capacity _effectiveCapacity;

    public required RecurringTrainingId RecurringTrainingId { get; init; }
    public required TrainingTitle EffectiveTitle { get => _effectiveTitle; init => _effectiveTitle = value; }
    public required TrainingDescription EffectiveDescription { get => _effectiveDescription; init => _effectiveDescription = value; }
    public required TimeSlot TimeSlot { get => _timeSlot; init => _timeSlot = value; }
    public required Capacity EffectiveCapacity { get => _effectiveCapacity; init => _effectiveCapacity = value; }
    public Visibility EffectiveVisibility { get; private set; }
    public SessionStatus Status { get; private set; }
    public SessionOverrides Overrides { get; private set; } = SessionOverrides.None;
    public IReadOnlyList<Participant> Participants => _participants.AsReadOnly();
    public IReadOnlyList<MemberId> EffectiveTrainerIds => _effectiveTrainerIds.AsReadOnly();
    public IReadOnlyList<RoomRequirement> EffectiveRoomRequirements => _effectiveRoomRequirements.AsReadOnly();
    public DateTimeOffset CreatedAt { get; private set; }

    [SetsRequiredMembers]
    private TrainingSession()
    {
        _effectiveTitle = default!;
        _effectiveDescription = default!;
        _timeSlot = default!;
        _effectiveCapacity = default!;
        _participantManager = new ParticipantManager(_participants);
    }

    public static TrainingSession CreateFromTemplate(
        RecurringTrainingId recurringTrainingId,
        TimeSlot timeSlot,
        TrainingTemplate template)
    {
        Guard.AgainstNull(recurringTrainingId, nameof(recurringTrainingId));
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        Guard.AgainstNull(template, nameof(template));

        var session = new TrainingSession
        {
            Id = TrainingSessionId.Create(),
            RecurringTrainingId = recurringTrainingId,
            TimeSlot = timeSlot,
            EffectiveTitle = template.Title,
            EffectiveDescription = template.Description,
            EffectiveCapacity = template.Capacity,
            EffectiveVisibility = template.Visibility,
            Status = SessionStatus.Scheduled,
            CreatedAt = DateTimeOffset.UtcNow
        };

        session._effectiveTrainerIds.AddRange(template.TrainerIds);
        session._effectiveRoomRequirements.AddRange(template.RoomRequirements);

        return session;
    }

    public void ApplyOverrides(SessionOverrides overrides)
    {
        Guard.AgainstNull(overrides, nameof(overrides));

        if (Status != SessionStatus.Scheduled)
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "apply overrides");

        Overrides = overrides;

        if (overrides.Title is not null) _effectiveTitle = overrides.Title;
        if (overrides.Description is not null) _effectiveDescription = overrides.Description;
        if (overrides.Capacity is not null) _effectiveCapacity = overrides.Capacity;
        if (overrides.Visibility.HasValue) EffectiveVisibility = overrides.Visibility.Value;

        if (overrides.TrainerIds is not null)
        {
            _effectiveTrainerIds.Clear();
            _effectiveTrainerIds.AddRange(overrides.TrainerIds);
        }

        if (overrides.RoomRequirements is not null)
        {
            _effectiveRoomRequirements.Clear();
            _effectiveRoomRequirements.AddRange(overrides.RoomRequirements);
        }
    }

    public void ResetToTemplate(TrainingTemplate template)
    {
        Guard.AgainstNull(template, nameof(template));

        if (Status != SessionStatus.Scheduled)
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "reset to template");

        Overrides = SessionOverrides.None;
        _effectiveTitle = template.Title;
        _effectiveDescription = template.Description;
        _effectiveCapacity = template.Capacity;
        EffectiveVisibility = template.Visibility;

        _effectiveTrainerIds.Clear();
        _effectiveTrainerIds.AddRange(template.TrainerIds);

        _effectiveRoomRequirements.Clear();
        _effectiveRoomRequirements.AddRange(template.RoomRequirements);
    }

    public void Cancel(string reason)
    {
        Guard.AgainstNullOrWhiteSpace(reason, nameof(reason));

        if (Status is SessionStatus.Canceled or SessionStatus.Completed)
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "cancel");

        Status = SessionStatus.Canceled;

        foreach (var p in _participants.Where(p => p.IsActive))
            p.Cancel();

        AddDomainEvent(new TrainingSessionCanceledEvent(Id, RecurringTrainingId, reason, DateTimeOffset.UtcNow));
    }

    public void Complete()
    {
        if (Status != SessionStatus.Scheduled)
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "complete");

        Status = SessionStatus.Completed;
    }

    public void AddParticipant(MemberId memberId)
    {
        Guard.AgainstNull(memberId, nameof(memberId));

        if (Status != SessionStatus.Scheduled)
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "add participant");

        var (participant, wasWaitlisted) = _participantManager.AddParticipant(memberId, EffectiveCapacity);

        AddDomainEvent(new ParticipantJoinedEvent(
            Id.Value,
            memberId,
            wasWaitlisted ? ParticipationStatus.Waitlisted : ParticipationStatus.Confirmed,
            DateTimeOffset.UtcNow));
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

        if (Status is not (SessionStatus.Scheduled or SessionStatus.Completed))
            throw new InvalidEntityStateException(nameof(TrainingSession), Status.ToString(), "record attendance");

        _participantManager.RecordAttendance(memberId, attended);

        AddDomainEvent(new AttendanceRecordedEvent(Id.Value, memberId, attended, DateTimeOffset.UtcNow));
    }

    public int ConfirmedParticipantCount => _participantManager.ConfirmedCount;
    public int WaitlistCount => _participantManager.WaitlistCount;
}
