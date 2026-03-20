using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Training.Domain.ValueObjects;

namespace TrainingOrganizer.Training.Domain.Events;

public sealed record TrainingPublishedEvent(
    TrainingId TrainingId,
    IReadOnlyList<RoomRequirement> RoomRequirements,
    DateTimeOffset OccurredAt) : IDomainEvent;
