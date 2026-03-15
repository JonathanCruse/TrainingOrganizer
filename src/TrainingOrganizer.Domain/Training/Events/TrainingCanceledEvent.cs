using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record TrainingCanceledEvent(
    TrainingId TrainingId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
