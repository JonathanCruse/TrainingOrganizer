using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Training.ValueObjects;

namespace TrainingOrganizer.Domain.Training.Events;

public sealed record TrainingPublishedEvent(
    TrainingId TrainingId,
    IReadOnlyList<RoomRequirement> RoomRequirements,
    DateTimeOffset OccurredAt) : IDomainEvent;
