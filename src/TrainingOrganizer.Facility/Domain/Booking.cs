using System.Diagnostics.CodeAnalysis;
using TrainingOrganizer.SharedKernel.Domain;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain.Enums;
using TrainingOrganizer.Facility.Domain.Events;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain;

public sealed class Booking : AggregateRoot<BookingId>
{
    private TimeSlot _timeSlot;

    public required RoomId RoomId { get; init; }
    public required LocationId LocationId { get; init; }
    public required TimeSlot TimeSlot { get => _timeSlot; init => _timeSlot = value; }
    public BookingStatus Status { get; private set; }
    public required BookingReference Reference { get; init; }
    public DateTimeOffset CreatedAt { get; private set; }
    public Guid CreatedBy { get; private set; }

    [SetsRequiredMembers]
    private Booking()
    {
        _timeSlot = default!;
    }

    public static Booking Create(
        RoomId roomId,
        LocationId locationId,
        TimeSlot timeSlot,
        BookingReference reference,
        Guid createdBy)
    {
        Guard.AgainstNull(roomId, nameof(roomId));
        Guard.AgainstNull(locationId, nameof(locationId));
        Guard.AgainstNull(timeSlot, nameof(timeSlot));
        Guard.AgainstNull(reference, nameof(reference));

        var booking = new Booking
        {
            Id = BookingId.Create(),
            RoomId = roomId,
            LocationId = locationId,
            TimeSlot = timeSlot,
            Status = BookingStatus.Active,
            Reference = reference,
            CreatedAt = DateTimeOffset.UtcNow,
            CreatedBy = createdBy
        };

        booking.AddDomainEvent(new BookingCreatedEvent(
            booking.Id, roomId, locationId, timeSlot, DateTimeOffset.UtcNow));

        return booking;
    }

    public void Cancel()
    {
        if (Status == BookingStatus.Canceled)
            throw new InvalidEntityStateException(nameof(Booking), Status.ToString(), "cancel");

        Status = BookingStatus.Canceled;

        AddDomainEvent(new BookingCanceledEvent(Id, DateTimeOffset.UtcNow));
    }

    public void Reschedule(TimeSlot newTimeSlot)
    {
        Guard.AgainstNull(newTimeSlot, nameof(newTimeSlot));

        if (Status != BookingStatus.Active)
            throw new InvalidEntityStateException(nameof(Booking), Status.ToString(), "reschedule");

        _timeSlot = newTimeSlot;
    }

    public bool IsActive => Status == BookingStatus.Active;
}
