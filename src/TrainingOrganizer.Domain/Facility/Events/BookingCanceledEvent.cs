using TrainingOrganizer.Domain.Common;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Facility.Events;

public sealed record BookingCanceledEvent(
    BookingId BookingId,
    DateTimeOffset OccurredAt) : IDomainEvent;
