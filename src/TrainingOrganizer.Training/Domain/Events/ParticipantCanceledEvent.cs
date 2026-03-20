using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Membership.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record ParticipantCanceledEvent(
    Guid TrainingOrSessionId,
    MemberId MemberId,
    DateTimeOffset OccurredAt) : IDomainEvent;
