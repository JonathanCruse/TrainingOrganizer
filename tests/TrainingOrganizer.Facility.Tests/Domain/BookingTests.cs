using FluentAssertions;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.SharedKernel.Domain.Exceptions;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.Enums;
using TrainingOrganizer.Facility.Domain.Events;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Tests.Domain;

public class BookingTests
{
    private static TimeSlot CreateTimeSlot()
    {
        var start = DateTimeOffset.UtcNow.AddDays(7);
        return new TimeSlot(start, start.AddHours(2));
    }

    private static Booking CreateActiveBooking()
    {
        var booking = Booking.Create(
            RoomId.Create(),
            LocationId.Create(),
            CreateTimeSlot(),
            new BookingReference(BookingReferenceType.Training, Guid.NewGuid()),
            Guid.NewGuid());
        booking.ClearDomainEvents();
        return booking;
    }

    // --- Create ---

    [Fact]
    public void Create_ValidData_ReturnsActiveBooking()
    {
        var roomId = RoomId.Create();
        var locationId = LocationId.Create();
        var timeSlot = CreateTimeSlot();
        var reference = new BookingReference(BookingReferenceType.Training, Guid.NewGuid());
        var createdBy = Guid.NewGuid();

        var booking = Booking.Create(roomId, locationId, timeSlot, reference, createdBy);

        booking.Id.Should().NotBeNull();
        booking.Status.Should().Be(BookingStatus.Active);
        booking.RoomId.Should().Be(roomId);
        booking.LocationId.Should().Be(locationId);
        booking.TimeSlot.Should().Be(timeSlot);
        booking.Reference.Should().Be(reference);
        booking.IsActive.Should().BeTrue();
    }

    [Fact]
    public void Create_ValidData_RaisesBookingCreatedEvent()
    {
        var booking = Booking.Create(
            RoomId.Create(),
            LocationId.Create(),
            CreateTimeSlot(),
            new BookingReference(BookingReferenceType.Manual, Guid.NewGuid()),
            Guid.NewGuid());

        booking.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BookingCreatedEvent>()
            .Which.BookingId.Should().Be(booking.Id);
    }

    // --- Cancel ---

    [Fact]
    public void Cancel_ActiveBooking_TransitionsToCanceled()
    {
        var booking = CreateActiveBooking();

        booking.Cancel();

        booking.Status.Should().Be(BookingStatus.Canceled);
        booking.IsActive.Should().BeFalse();
    }

    [Fact]
    public void Cancel_ActiveBooking_RaisesBookingCanceledEvent()
    {
        var booking = CreateActiveBooking();

        booking.Cancel();

        booking.DomainEvents.Should().ContainSingle()
            .Which.Should().BeOfType<BookingCanceledEvent>()
            .Which.BookingId.Should().Be(booking.Id);
    }

    [Fact]
    public void Cancel_AlreadyCanceledBooking_ThrowsInvalidEntityStateException()
    {
        var booking = CreateActiveBooking();
        booking.Cancel();

        var act = () => booking.Cancel();

        act.Should().Throw<InvalidEntityStateException>();
    }

    // --- Reschedule ---

    [Fact]
    public void Reschedule_ActiveBooking_UpdatesTimeSlot()
    {
        var booking = CreateActiveBooking();
        var newTimeSlot = new TimeSlot(
            DateTimeOffset.UtcNow.AddDays(14),
            DateTimeOffset.UtcNow.AddDays(14).AddHours(3));

        booking.Reschedule(newTimeSlot);

        booking.TimeSlot.Should().Be(newTimeSlot);
    }

    [Fact]
    public void Reschedule_CanceledBooking_ThrowsInvalidEntityStateException()
    {
        var booking = CreateActiveBooking();
        booking.Cancel();

        var newTimeSlot = new TimeSlot(
            DateTimeOffset.UtcNow.AddDays(14),
            DateTimeOffset.UtcNow.AddDays(14).AddHours(3));

        var act = () => booking.Reschedule(newTimeSlot);

        act.Should().Throw<InvalidEntityStateException>();
    }
}
