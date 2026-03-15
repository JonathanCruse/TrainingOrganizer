using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record RecurringTrainingTemplateUpdatedEvent(
    RecurringTrainingId RecurringTrainingId,
    TrainingTemplate NewTemplate,
    DateTimeOffset OccurredAt) : IDomainEvent;
