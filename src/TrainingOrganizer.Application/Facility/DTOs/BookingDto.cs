using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Application.Facility.DTOs;

public sealed record BookingDto(
    Guid Id,
    Guid RoomId,
    Guid LocationId,
    DateTimeOffset Start,
    DateTimeOffset End,
    BookingStatus Status,
    BookingReferenceType ReferenceType,
    Guid ReferenceId,
    DateTimeOffset CreatedAt,
    Guid CreatedBy)
{
    public static BookingDto FromDomain(Booking booking) => new(
        booking.Id.Value,
        booking.RoomId.Value,
        booking.LocationId.Value,
        booking.TimeSlot.Start,
        booking.TimeSlot.End,
        booking.Status,
        booking.Reference.ReferenceType,
        booking.Reference.ReferenceId,
        booking.CreatedAt,
        booking.CreatedBy);
}
