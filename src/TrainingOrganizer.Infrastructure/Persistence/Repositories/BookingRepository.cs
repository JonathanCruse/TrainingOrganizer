using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.Enums;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class BookingRepository : IBookingRepository
{
    private readonly MongoDbContext _context;

    public BookingRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Booking?> GetByIdAsync(BookingId id, CancellationToken ct = default)
    {
        var filter = Builders<BookingDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.Bookings.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<Booking>> GetByRoomAndDateRangeAsync(
        RoomId roomId, DateTimeOffset from, DateTimeOffset to, CancellationToken ct = default)
    {
        var filter = Builders<BookingDocument>.Filter.And(
            Builders<BookingDocument>.Filter.Eq(d => d.RoomId, roomId.Value),
            Builders<BookingDocument>.Filter.Lt(d => d.TimeSlotStart, to),
            Builders<BookingDocument>.Filter.Gt(d => d.TimeSlotEnd, from),
            Builders<BookingDocument>.Filter.Eq(d => d.Status, BookingStatus.Active.ToString()));

        var documents = await _context.Bookings
            .Find(filter)
            .Sort(Builders<BookingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Booking>> GetByReferenceAsync(
        BookingReference reference, CancellationToken ct = default)
    {
        var filter = Builders<BookingDocument>.Filter.And(
            Builders<BookingDocument>.Filter.Eq(d => d.ReferenceType, reference.ReferenceType.ToString()),
            Builders<BookingDocument>.Filter.Eq(d => d.ReferenceId, reference.ReferenceId));

        var documents = await _context.Bookings
            .Find(filter)
            .Sort(Builders<BookingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<IReadOnlyList<Booking>> GetActiveByRoomAsync(
        RoomId roomId, CancellationToken ct = default)
    {
        var filter = Builders<BookingDocument>.Filter.And(
            Builders<BookingDocument>.Filter.Eq(d => d.RoomId, roomId.Value),
            Builders<BookingDocument>.Filter.Eq(d => d.Status, BookingStatus.Active.ToString()));

        var documents = await _context.Bookings
            .Find(filter)
            .Sort(Builders<BookingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<PagedList<Booking>> GetPagedAsync(
        int page, int pageSize, RoomId? roomId, DateTimeOffset? from, DateTimeOffset? to, CancellationToken ct = default)
    {
        var filterBuilder = Builders<BookingDocument>.Filter;
        var filter = filterBuilder.Empty;

        if (roomId is not null)
        {
            filter &= filterBuilder.Eq(d => d.RoomId, roomId.Value);
        }

        if (from.HasValue)
        {
            filter &= filterBuilder.Gte(d => d.TimeSlotStart, from.Value);
        }

        if (to.HasValue)
        {
            filter &= filterBuilder.Lte(d => d.TimeSlotStart, to.Value);
        }

        var totalCount = await _context.Bookings.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.Bookings
            .Find(filter)
            .Sort(Builders<BookingDocument>.Sort.Ascending(d => d.TimeSlotStart))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<Booking>(items, page, pageSize, (int)totalCount);
    }

    public async Task AddAsync(Booking booking, CancellationToken ct = default)
    {
        var document = BookingDocument.FromDomain(booking);
        await _context.Bookings.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(Booking booking, CancellationToken ct = default)
    {
        var document = BookingDocument.FromDomain(booking);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<BookingDocument>.Filter.And(
            Builders<BookingDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<BookingDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.Bookings.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(Booking), booking.Id);
    }
}
