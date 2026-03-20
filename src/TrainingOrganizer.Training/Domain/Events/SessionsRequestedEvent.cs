using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record SessionsRequestedEvent(
    RecurringTrainingId RecurringTrainingId,
    TrainingTemplate Template,
    IReadOnlyList<DateOnly> OccurrenceDates,
    RecurrenceRule RecurrenceRule,
    DateTimeOffset OccurredAt) : IDomainEvent;
