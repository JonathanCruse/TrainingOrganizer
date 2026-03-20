using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record RecurringTrainingTemplateUpdatedEvent(
    RecurringTrainingId RecurringTrainingId,
    TrainingTemplate NewTemplate,
    DateTimeOffset OccurredAt) : IDomainEvent;
