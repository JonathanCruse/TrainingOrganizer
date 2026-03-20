using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Domain.Services;

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
