using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;
using TrainingOrganizer.Domain.Training.Enums;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record ParticipantJoinedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    ParticipationStatus Status,
    DateTimeOffset OccurredAt) : IDomainEvent;
