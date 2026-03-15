using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Facility.Events;

public sealed record BookingCreatedEvent(
    BookingId BookingId,
    RoomId RoomId,
    LocationId LocationId,
    TimeSlot TimeSlot,
    DateTimeOffset OccurredAt) : IDomainEvent;
