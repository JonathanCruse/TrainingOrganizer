using MongoDB.Driver;
using TrainingOrganizer.Application.Common.Models;
using TrainingOrganizer.Application.Facility.Repositories;
using TrainingOrganizer.Domain.Facility;
using TrainingOrganizer.Domain.Facility.ValueObjects;
using TrainingOrganizer.Infrastructure.Persistence.Documents;

namespace TrainingOrganizer.Infrastructure.Persistence.Repositories;

public sealed class LocationRepository : ILocationRepository
{
    private readonly MongoDbContext _context;

    public LocationRepository(MongoDbContext context)
    {
        _context = context;
    }

    public async Task<Location?> GetByIdAsync(LocationId id, CancellationToken ct = default)
    {
        var filter = Builders<LocationDocument>.Filter.Eq(d => d.Id, id.Value);
        var document = await _context.Locations.Find(filter).FirstOrDefaultAsync(ct);
        return document?.ToDomain();
    }

    public async Task<IReadOnlyList<Location>> GetAllAsync(CancellationToken ct = default)
    {
        var documents = await _context.Locations
            .Find(Builders<LocationDocument>.Filter.Empty)
            .Sort(Builders<LocationDocument>.Sort.Ascending(d => d.Name))
            .ToListAsync(ct);

        return documents.Select(d => d.ToDomain()).ToList();
    }

    public async Task<PagedList<Location>> GetPagedAsync(
        int page, int pageSize, CancellationToken ct = default)
    {
        var filter = Builders<LocationDocument>.Filter.Empty;

        var totalCount = await _context.Locations.CountDocumentsAsync(filter, cancellationToken: ct);
        var documents = await _context.Locations
            .Find(filter)
            .Sort(Builders<LocationDocument>.Sort.Ascending(d => d.Name))
            .Skip((page - 1) * pageSize)
            .Limit(pageSize)
            .ToListAsync(ct);

        var items = documents.Select(d => d.ToDomain()).ToList();
        return new PagedList<Location>(items, page, pageSize, (int)totalCount);
    }

    public async Task AddAsync(Location location, CancellationToken ct = default)
    {
        var document = LocationDocument.FromDomain(location);
        await _context.Locations.InsertOneAsync(document, cancellationToken: ct);
    }

    public async Task UpdateAsync(Location location, CancellationToken ct = default)
    {
        var document = LocationDocument.FromDomain(location);
        var expectedVersion = document.Version;
        document.Version = expectedVersion + 1;

        var filter = Builders<LocationDocument>.Filter.And(
            Builders<LocationDocument>.Filter.Eq(d => d.Id, document.Id),
            Builders<LocationDocument>.Filter.Eq(d => d.Version, expectedVersion));

        var result = await _context.Locations.ReplaceOneAsync(filter, document, cancellationToken: ct);

        if (result.ModifiedCount == 0)
            throw new ConcurrencyException(nameof(Location), location.Id);
    }
}
