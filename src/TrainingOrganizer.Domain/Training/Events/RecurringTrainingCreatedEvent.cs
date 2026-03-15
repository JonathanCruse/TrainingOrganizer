using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record RecurringTrainingCreatedEvent(
    RecurringTrainingId RecurringTrainingId,
    DateTimeOffset OccurredAt) : IDomainEvent;
