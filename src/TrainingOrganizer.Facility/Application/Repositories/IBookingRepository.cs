using TrainingOrganizer.SharedKernel.Application.Models;
using TrainingOrganizer.Facility.Domain;
using TrainingOrganizer.Facility.Domain.ValueObjects;

namespace TrainingOrganizer.Facility.Application.Repositories;

public interface IBookingRepository
{
    Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByRoomAndDateRangeAsync(RoomId roomId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetByReferenceAsync(BookingReference reference, CancellationToken ct = default);
    Task<IReadOnlyList<Booking>> GetActiveByRoomAsync(RoomId roomId, CancellationToken ct = default);
    Task<PagedList<Booking>> GetPagedAsync(int page, int pageSize, RoomId? roomId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default);
    Task AddAsync(Booking booking, CancellationToken ct = default);
    Task UpdateAsync(Booking booking, CancellationToken ct = default);
}
