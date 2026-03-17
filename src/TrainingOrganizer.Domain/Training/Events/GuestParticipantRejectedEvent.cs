using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Membership.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record GuestParticipantRejectedEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    DateTimeOffset OccurredAt) : IDomainEvent;
