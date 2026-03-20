using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain.Events;

public sealed record BookingCanceledEvent(
    BookingId BookingId,
    DateTimeOffset OccurredAt) : IDomainEvent;
