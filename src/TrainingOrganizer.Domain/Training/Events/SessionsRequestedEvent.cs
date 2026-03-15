using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record SessionsRequestedEvent(
    RecurringTrainingId RecurringTrainingId,
    TrainingTemplate Template,
    IReadOnlyList<DateOnly> OccurrenceDates,
    RecurrenceRule RecurrenceRule,
    DateTimeOffset OccurredAt) : IDomainEvent;
