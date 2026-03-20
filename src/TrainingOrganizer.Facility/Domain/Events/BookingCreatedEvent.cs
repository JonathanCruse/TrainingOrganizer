using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain.Events;

public sealed record BookingCreatedEvent(
    BookingId BookingId,
    RoomId RoomId,
    LocationId LocationId,
    TimeSlot TimeSlot,
    DateTimeOffset OccurredAt) : IDomainEvent;
