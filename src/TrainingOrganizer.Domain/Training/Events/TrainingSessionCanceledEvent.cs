using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record TrainingSessionCanceledEvent(
    TrainingSessionId TrainingSessionId,
    RecurringTrainingId RecurringTrainingId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
