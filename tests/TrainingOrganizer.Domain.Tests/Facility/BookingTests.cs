using FluentAssertions;
using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Exceptions;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.Events;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Domain.Tests.TestHelpers;

namespace TrainingOrganizer.Domain.Tests.Facility;

public class BookingTests
{
    private static Booking CreateActiveBooking()
    {
        var booking = Booking.Create(
            RoomId.Create(),
            LocationId.Create(),
            TrainingFactory.CreateTimeSlot(),
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
        var timeSlot = TrainingFactory.CreateTimeSlot();
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
            TrainingFactory.CreateTimeSlot(),
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
