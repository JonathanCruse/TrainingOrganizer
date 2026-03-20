using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;
using TrainingOrganizer.Training.Domain.Enums;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record ParticipantJoinedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    ParticipationStatus Status,
    DateTimeOffset OccurredAt) : IDomainEvent;
