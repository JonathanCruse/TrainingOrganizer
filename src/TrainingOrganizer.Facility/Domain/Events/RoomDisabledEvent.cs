using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain.Events;

public sealed record RoomDisabledEvent(
    LocationId LocationId,
    RoomId RoomId,
    DateTimeOffset OccurredAt) : IDomainEvent;
