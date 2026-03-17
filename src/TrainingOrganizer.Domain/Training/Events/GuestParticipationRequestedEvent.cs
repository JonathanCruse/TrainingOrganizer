using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record GuestParticipationRequestedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    DateTimeOffset OccurredAt) : IDomainEvent;
