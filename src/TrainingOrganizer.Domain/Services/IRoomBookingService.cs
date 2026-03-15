using TrainingOrganizer.Domain.Common.ValueObjects;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;

namespace TrainingOrganizer.Domain.Services;

public interface IRoomBookingService
{
    Task<bool> HasConflictAsync(
        RoomId roomId,
        TimeSlot timeSlot,
        BookingId? excludeBookingId = null,
        CancellationToken cancellationToken = default);

    Task<Booking> BookRoomAsync(
        RoomId roomId,
        LocationId locationId,
        TimeSlot timeSlot,
        BookingReference reference,
        Guid createdBy,
        CancellationToken cancellationToken = default);

    Task CancelBookingsForReferenceAsync(
        BookingReference reference,
        CancellationToken cancellationToken = default);
}
