using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record TrainingSessionCanceledEvent(
    TrainingSessionId TrainingSessionId,
    RecurringTrainingId RecurringTrainingId,
    string Reason,
    DateTimeOffset OccurredAt) : IDomainEvent;
