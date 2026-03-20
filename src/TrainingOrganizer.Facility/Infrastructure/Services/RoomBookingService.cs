using MongoDB.Driver;
using TrainingOrganizer.SharedKernel.Infrastructure.Persistence;
using TrainingOrganizer.Facility.Application.Repositories;
using TrainingOrganizer.SharedKernel.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.Enums;
using TrainingOrganizer.Facility.Domain.ValueObjects;
using TrainingOrganizer.Facility.Domain.Services;
using TrainingOrganizer.Facility.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Facility.Infrastructure.Services;

public sealed class RoomBookingService : IRoomBookingService
{
    private readonly MongoDbContext _context;
    private readonly IBookingRepository _bookingRepository;

    private IMongoCollection<BookingDocument> Bookings => _context.Database.GetCollection<BookingDocument>("bookings");

    public RoomBookingService(MongoDbContext context, IBookingRepository bookingRepository)
    {
        _context = context;
        _bookingRepository = bookingRepository;
    }

    public async Task<bool> HasConflictAsync(
        RoomId roomId, TimeSlot timeSlot, BookingId? excludeBookingId = null,
        CancellationToken cancellationToken = default)
    {
        var filterBuilder = Builders<BookingDocument>.Filter;

        // Find active bookings for the same room that overlap with the given time slot
        var filter = filterBuilder.And(
            filterBuilder.Eq(d => d.RoomId, roomId.Value),
            filterBuilder.Eq(d => d.Status, BookingStatus.Active.ToString()),
            filterBuilder.Lt(d => d.TimeSlotStart, timeSlot.End),
            filterBuilder.Gt(d => d.TimeSlotEnd, timeSlot.Start));

        if (excludeBookingId is not null)
        {
            filter &= filterBuilder.Ne(d => d.Id, excludeBookingId.Value);
        }

        var count = await Bookings.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
        return count > 0;
    }

    public async Task<Booking> BookRoomAsync(
        RoomId roomId, LocationId locationId, TimeSlot timeSlot,
        BookingReference reference, Guid createdBy,
        CancellationToken cancellationToken = default)
    {
        var hasConflict = await HasConflictAsync(roomId, timeSlot, cancellationToken: cancellationToken);

        if (hasConflict)
            throw new InvalidOperationException(
                $"Room '{roomId}' already has an active booking that overlaps with the requested time slot.");

        var booking = Booking.Create(roomId, locationId, timeSlot, reference, createdBy);
        await _bookingRepository.AddAsync(booking, cancellationToken);

        return booking;
    }

    public async Task CancelBookingsForReferenceAsync(
        BookingReference reference, CancellationToken cancellationToken = default)
    {
        var bookings = await _bookingRepository.GetByReferenceAsync(reference, cancellationToken);

        foreach (var booking in bookings.Where(b => b.IsActive))
        {
            booking.Cancel();
            await _bookingRepository.UpdateAsync(booking, cancellationToken);
        }
    }
}
