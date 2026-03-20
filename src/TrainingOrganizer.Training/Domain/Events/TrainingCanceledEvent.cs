using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record TrainingCanceledEvent(
    TrainingId TrainingId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
