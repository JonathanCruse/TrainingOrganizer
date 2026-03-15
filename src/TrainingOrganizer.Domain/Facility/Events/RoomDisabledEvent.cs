using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Facility.Events;

public sealed record RoomDisabledEvent(
    LocationId LocationId,
    RoomId RoomId,
    DateTimeOffset OccurredAt) : IDomainEvent;
