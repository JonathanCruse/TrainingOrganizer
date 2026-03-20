using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record TrainingCompletedEvent(
    TrainingId TrainingId,
    DateTimeOffset OccurredAt) : IDomainEvent;
